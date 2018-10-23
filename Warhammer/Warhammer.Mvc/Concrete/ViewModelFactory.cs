using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Concrete
{
    public class ViewModelFactory : IViewModelFactory
    {
        protected Player CurrentPlayer => _data.MyPlayer();
        readonly IAuthenticatedDataProvider _data;
        private readonly UrlHelper _urlHelper;
        

        public ViewModelFactory(UrlHelper urlHelper, IAuthenticatedDataProvider data)
        {
            _data = data;
            _urlHelper = urlHelper;
        }

        public ActiveTextSessionViewModel MakeActiveTextSessionViewModel()
        {
            ActiveTextSessionViewModel model = new ActiveTextSessionViewModel();

            List<Session> myOpenTestSessions = _data.MyOpenTextSessions().ToList();

            foreach (Session myOpenTestSession in myOpenTestSessions)
            {
                OpenSessionViewModel sessionViewModel = new OpenSessionViewModel {Session = myOpenTestSession, Status = OpenSessionStatus.Stale };

                if(myOpenTestSession.PageViews.Any(v => v.PlayerId == CurrentPlayer.Id && v.Viewed < myOpenTestSession.LastPostTime))
                {
                    sessionViewModel.Status = OpenSessionStatus.Updated;
                    sessionViewModel.IsUpdated = true;
                }

                if (_data.TextSessionsWhereItisMyTurn().Contains(myOpenTestSession))
                {
                    sessionViewModel.Status = OpenSessionStatus.MyTurn;
                }
                model.OpenSessions.Add(sessionViewModel);
            }

            model.OpenSessions =
                model.OpenSessions.OrderByDescending(m => m.Status == OpenSessionStatus.MyTurn)
                    .ThenBy(m => m.LastPostTime)
                    .ToList();

            return model;
        }

        public PersonStatViewModel MakeStatModel(Person person)
        {
            PersonStatViewModel model = new PersonStatViewModel {PersonId = person.Id, CharacterName = person.ShortName };

            model.Stats = person.Stats;

            if (!model.StatsCreated)
            {
                if (_data.SiteHasFeature(Feature.CrowStats))
                {
                    foreach (int statId in Enum.GetValues(typeof(StatName)).AsQueryable())
                    {
                        if (statId < 100)
                        {
                            StatName stat = (StatName) statId;
                            if (!model.Stats.ContainsKey(stat))
                            {
                                model.Stats.Add(stat, 0);
                            }
                        }
                    }
                }

                if (_data.SiteHasFeature(Feature.FuHammerStats))
                {
                    foreach (int statId in Enum.GetValues(typeof(StatName)).AsQueryable())
                    {
                        if (statId < 200 && statId > 100)
                        {
                            StatName stat = (StatName) statId;
                            if (!model.Stats.ContainsKey(stat))
                            {
                                model.Stats.Add(stat, 0);
                            }
                        }
                    }
                }
            }

            model.Descriptors = person.DescriptorNames;
            model.Roles = person.RoleNames;
            model.CurrentXp = person.CurrentXp;
            model.StatCost = person.StatCost;
            model.CanBuyStat = person.CanBuyStat;
            model.RoleCost = person.RoleCost;
            model.CanBuyRole = person.CanBuyRole;
            model.DescriptorCost = person.DescriptorCost;
            model.CanBuyDescriptor = person.CanBuyDescriptor;
            model.XpSpent = person.XpSpent;
            model.MaySpendXp = !person.IsDead && model.StatsCreated;
            model.IsNpc = person.IsNpc;

            model.ShowDescriptors = _data.SiteHasFeature(Feature.PersonDescriptors);
            model.ShowRoles = _data.SiteHasFeature(Feature.PersonRoles);
            model.ShowStats = _data.SiteHasFeature(Feature.SimpleStats);
            model.IsCrow = _data.SiteHasFeature(Feature.CrowStats);
            model.IsFuHammer = _data.SiteHasFeature(Feature.FuHammerStats);

            return model;
        }

        public MenuViewModel MakeMenu()
        {
            if (_data.CurrentCampaignId == 0)
            {
                return NoCampaignMenu();
            }

            MenuViewModel model = new MenuViewModel();

            List<MenuItemViewModel> usefulSubMenu = MakeUsefulSubmenu();
            List<MenuItemViewModel> otherSitesSubMenu = MakeOtherSitesSubmenu();
            List<MenuItemViewModel> featuresMenu = MakeFeaturesSubmenu();
            List<MenuItemViewModel> peopleSubMenu = MakePeopleSubmenu();
            List<MenuItemViewModel> adminSubMenu = MakeAdminSubmenu();
            List<MenuItemViewModel> gmSubMenu = MakeGmSubMenu();
            List<MenuItemViewModel> createMenu = MakeCreateMenu();
            

            if (createMenu.Any())
            {
                model.CreateMenu = new List<MenuItemViewModel>
                {
                    new MenuItemViewModel
                    {
                        AltText = "Create Stuff",
                        Name = "Create New",
                        SubMenu = createMenu
                    }
                };
            }

            if (usefulSubMenu.Any())
            {
                model.LeftMenu.Add(new MenuItemViewModel
                {
                    Name = "Useful Pages",
                    Url = "#",
                    SubMenu = usefulSubMenu
                });
            }

            if (featuresMenu.Any())
            {
                model.LeftMenu.Add(new MenuItemViewModel
                {
                    Name = "Features",
                    Url = "#",
                    SubMenu = featuresMenu
                });
            }

            if (gmSubMenu.Any())
            {
                model.LeftMenu.Add(new MenuItemViewModel
                {
                    Name = "GM Stuff",
                    Url = "#",
                    SubMenu = gmSubMenu
                });
            }

            if (adminSubMenu.Any())
            {
                model.LeftMenu.Add(new MenuItemViewModel
                {
                    Name = "Administration",
                    Url = "#",
                    SubMenu = adminSubMenu
                });
            }

            if (peopleSubMenu.Any())
            {
                model.LeftMenu.Add(new MenuItemViewModel
                {
                    Name = "People",
                    Url = "#",
                    SubMenu = peopleSubMenu
                });
            }

            if (_data.SiteHasFeature(Feature.UserSettings) && !_data.CurrentUserIsGuest)
            {
                model.RightMenu.Add(new MenuItemViewModel
                {
                    Name = "",
                    AltText = "Settings",
                    Url = _urlHelper.Action("Settings", "Home"),
                    IconUrl = _urlHelper.Content("~/Content/Images/Settings.png"),
                });
            }

            if (_data.SiteHasFeature(Feature.RecentItemsMenu))
            {
                List<PageLinkModel> recentPages = _data.GetRecentViewedPages();
                model.RightMenu.Add(new MenuItemViewModel
                {
                    Name = "",
                    AltText = "Settings",
                    Url = "#",
                    IconUrl = _urlHelper.Content("~/Content/Images/recent.png"),
                    SubMenu = recentPages.Select(l => new MenuItemViewModel
                    {
                        AltText = l.ShortName,
                        Name = l.FullName,
                        IconUrl = _urlHelper.Action("Image", "Page", new { id = l.Id }),
                        Url = _urlHelper.Action("Index", "Page", new { id = l.Id })

                    }).ToList()
                });
            }

            if (otherSitesSubMenu.Any())
            {
                model.RightMenu.Add(new MenuItemViewModel
                {
                    Name = "",
                    AltText = "Other Sites",
                    Url = "#",
                    IconUrl = _urlHelper.Content("~/Content/Images/globe.png"),
                    SubMenu = otherSitesSubMenu
                });
            }

            return model;
        }

        private List<MenuItemViewModel> MakeOtherSitesSubmenu()
        {
            List<MenuItemViewModel> model = new List<MenuItemViewModel>();
            if (_data.SiteHasFeature(Feature.ShowOtherSitesMenu))
            {
                List<CampaignDetail> campaigns = _data.GetPermittedCampaigns();
                foreach (CampaignDetail campaign in campaigns.Where(c => c.CampaignId != _data.CurrentCampaignId))
                {
                    model.Add(new MenuItemViewModel
                    {
                        Name = campaign.DisplayName,
                        AltText = campaign.Url,
                        Url = $"https://{campaign.Url}",
                    });
                }
            }
            return model;
        }

        private MenuViewModel NoCampaignMenu()
        {
            MenuViewModel model = new MenuViewModel {LeftMenu = new List<MenuItemViewModel>()};


            if (_data.CurrentUserIsAdmin)
            {
                model.LeftMenu.Add(new MenuItemViewModel
                {
                    Name = "",
                    AltText = "Create Campaign on this domain",
                    Url = _urlHelper.Action("CreateCampaign", "Admin"),
                    IconUrl = _urlHelper.Content("~/Content/Images/Settings.png"),
                    //  IconCssClass = "badge"
                });
            }
            return model;
        }

        private List<MenuItemViewModel> MakeCreateMenu()
        {
            List<MenuItemViewModel> menu = new List<MenuItemViewModel>();

            menu.Add(new MenuItemViewModel
            {
                Name = "New Person",
                Url = _urlHelper.Action("Person", "Create")
            });

            menu.Add(new MenuItemViewModel
            {
                Name = "New Session",
                Url = _urlHelper.Action("GameSession", "Create")
            });

            menu.Add(new MenuItemViewModel
            {
                Name = "New Session Log",
                Url = _urlHelper.Action("SessionLog", "Create")
            });

            menu.Add(new MenuItemViewModel
            {
                Name = "New Place / Location",
                Url = _urlHelper.Action("Place", "Create")
            });

            menu.Add(new MenuItemViewModel
            {
                Name = "Generic Page",
                Url = _urlHelper.Action("Page", "Create")
            });

            if (_data.CurrentPlayerIsGm || _data.CurrentUserIsAdmin)
            {
                menu.Add(new MenuItemViewModel
                {
                    Name = "New Trophy",
                    Url = _urlHelper.Action("EditTrophy", "Gm")
                });
            }

            return menu;
        }

        private List<MenuItemViewModel> MakeGmSubMenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();
            if (_data.CurrentPlayerIsGm)
            {
                if (_data.SiteHasFeature(Feature.AwardNominations))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "Review Award Nominations",
                        Url = _urlHelper.Action("OutstandingAwardNominations", "Gm"),
                    });
                }

                items.Add(new MenuItemViewModel
                {
                    Name = "Outstanding Xp",
                    Url = _urlHelper.Action("OutstandingXp", "Gm"),
                });
                items.Add(new MenuItemViewModel
                {
                    Name = "Xp To Spend",
                    Url = _urlHelper.Action("XpToSpend", "Gm"),
                });

                if (_data.SiteHasFeature(Feature.PersonAttributes))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "NPC Sheet",
                        Url = _urlHelper.Action("NpcSheet", "Gm"),
                    });
                }

                if (_data.SiteHasFeature(Feature.PriceList) && !_data.SiteHasFeature(Feature.PublicPrices))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "Price List",
                        Url = _urlHelper.Action("PriceList", "Home"),
                    });
                }

                if (_data.SiteHasFeature(Feature.RumourMill))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "Manage Rumours",
                        Url = _urlHelper.Action("Index", "Rumour"),
                    });
                }

                if (_data.SiteHasFeature(Feature.RandomItemGenerator))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "Random Items",
                        Url = _urlHelper.Action("Index", "Random"),
                    });
                }
            }

            return items;
        }

        private List<MenuItemViewModel> MakeAdminSubmenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();
            if (_data.CurrentUserIsAdmin)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Edit Features",
                    AltText = "Features",
                    Url = _urlHelper.Action("Features", "Admin"),
                    IconUrl = _urlHelper.Content("~/Content/Images/Features.png"),
                    //  IconCssClass = "badge"
                });
                items.Add(new MenuItemViewModel
                {
                    Name = "Logs",
                    AltText = "Logs",
                    Url = _urlHelper.Action("Log", "Admin"),
                    IconUrl = _urlHelper.Content("~/Content/Images/log.png"),
                    //  IconCssClass = "badge"
                });

                if (_data.SiteHasFeature(Feature.PriceList) && !_data.SiteHasFeature(Feature.PublicPrices))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "Price List",
                        Url = _urlHelper.Action("PriceList", "Home"),
                    });
                }

                if (_data.SiteHasFeature(Feature.RumourMill))
                {
                    items.Add(new MenuItemViewModel
                    {
                        Name = "Manage Rumours",
                        Url = _urlHelper.Action("Index", "Rumour"),
                    });
                }
                items.Add(new MenuItemViewModel
                {
                    Name = "Campaign Settings",
                    Url = _urlHelper.Action("CampaignSettings", "Admin"),
                });
            }

            return items;
        }

        private List<MenuItemViewModel> MakeFeaturesSubmenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();
            return items;
        }

        public UserSettingsViewModel MakeUserSettings()
        {
            UserSettingsViewModel model = new UserSettingsViewModel();

            if (_data.SiteHasFeature(Feature.ImmediateEmailer))
            {
                model.SectionsIds.Add(SettingSection.EmailNotifications);
            }

            if (_data.SiteHasFeature(Feature.ShadowMode))
            {
                model.SectionsIds.Add(SettingSection.ShadowMode);
            }

            if (_data.SiteHasFeature(Feature.AwardNominations))
            {
                model.SectionsIds.Add(SettingSection.AwardSettings);
            }

            return model;
        }

        public UserSettingsSectionViewModel Make(List<Setting> settingSection)
        {
            UserSettingsSectionViewModel model = new UserSettingsSectionViewModel();
            if (settingSection.Any())
            {
                model.SectionId =  settingSection.First().SectionId;
                model.SettingTitle = GetSettingTitle(settingSection.First().SettingSection);
                model.Settings = settingSection.Select(Make).ToList();
            }
            return model;
        }

        public PersonAssetsViewModel MakePersonAssetsViewModel(Person person)
        {
            PersonAssetsViewModel model = new PersonAssetsViewModel();
            model.PersonId = person.Id;
            model.Assets = person.Assets.ToList();
            model.AllowEdit = _data.CurrentPlayerIsGm || CurrentPlayer.Id == person.PlayerId;

            model.ShowUpkeep = _data.SiteHasFeature(Feature.WarhammerMoney);
            foreach (Asset asset in model.Assets)
            {
                asset.ShowUpkeep = model.ShowUpkeep;
            }

            return model;
        }

        public PlayerSessionControlsViewModel MakePlayerSessionControlsViewModel(Session session, Player player, bool playerIsGm)
        {
            PlayerSessionControlsViewModel viewModel = new PlayerSessionControlsViewModel()
            {
                Players = new List<SuspendPlayerItemViewModel>()
            };

            if (playerIsGm)
            {
                SuspendPlayerItemViewModel playerModel = new SuspendPlayerItemViewModel()
                {
                    SessionId = session.Id,
                    PlayerId = player.Id,
                    PlayerName = player.DisplayName + " (GM)",
                    PlayerSuspended = session.GmIsSuspended
                };
                viewModel.Players.Add(playerModel);
            }

            foreach (PostOrder postOrder in session.PostOrders.Where(p => p.PlayerId == player.Id || playerIsGm))
            {
                SuspendPlayerItemViewModel playerModel = new SuspendPlayerItemViewModel()
                {
                    SessionId = session.Id,
                    PlayerId = postOrder.PlayerId,
                    PlayerName = postOrder.Player.DisplayName,
                    PlayerSuspended = postOrder.IsSuspended
                };
                viewModel.Players.Add(playerModel);
            }

            return viewModel;            
        }

        public SessionGmViewModel MakeSessionGmViewModel(int id, List<Player> players, int gmId)
        {
            SessionGmViewModel model = new SessionGmViewModel();

            model.SessionId = id;
            model.Players = new SelectList(players, "Id", "DisplayName");
            model.SelectedGm = gmId;
            return model;
        }

        public PageControlsViewModel MakePageControlsViewModel(Page page, bool isAdmin, bool playerIsGm, int currentPlayerId, List<Player> players)
        {
            PageControlsViewModel model = new PageControlsViewModel
            {
                Page = page,
                PlayerIsAdmin = isAdmin,
                PlayerIsGm = playerIsGm,
                CurrentPlayerId = currentPlayerId,
                Players = new SelectList(players, "Id", "DisplayName"),
                CurrentCampaignId = _data.CurrentCampaignId
            };

            Session session = page as Session;
            if (session?.GmId != null)
            {
                model.SelectedGm = session.GmId;
            }

            model.IsCrowMk2 = _data.SiteHasFeature(Feature.PersonAttributes);

            return model;
        }

        public ManageAwardsViewModel MakeManageAwardsViewModel(List<Trophy> trophies, Person person)
        {
            ManageAwardsViewModel model = new ManageAwardsViewModel
            {
                Awards = person.Awards.ToList(),
                PersonId = person.Id,
                PersonName = person.FullName,
                Trophies = new SelectList(trophies.Where(t => t.TrophyType == TrophyType.DefaultAward || t.TrophyType == TrophyType.MainPartyBanner).OrderBy(t => t.QuickName), "Id", "QuickName")
            };
            return model;
        }

        public EditTrophyViewModel Make(Trophy trophy, bool playerIsGm, bool playerIsAdmin)
        {
            EditTrophyViewModel model = new EditTrophyViewModel();
            model.Trophy = trophy;
            model.CurrentCampaignOnly = trophy.CurrentCampaignOnly;
            model.CanEdit = playerIsAdmin || (playerIsGm && trophy.CurrentCampaignOnly);
            model.CanEditGlobal = playerIsAdmin;
            model.CanSetType = playerIsAdmin;
            return model;
        }

        public TrophyCabinetViewModel Make(List<Trophy> trophies, bool currentPlayerIsGm, bool playerIsAdmin)
        {
            TrophyCabinetViewModel model = new TrophyCabinetViewModel
            {
                Trophies = trophies,
                CanAdd = playerIsAdmin || currentPlayerIsGm,
                CanEditGlobal = playerIsAdmin,
                CanEdit = playerIsAdmin || currentPlayerIsGm
            };
            return model;
        }

        public NpcSheetViewModel MakeNpcSheetViewModel(Person npc)
        {
            Dictionary<string, int> roles = new Dictionary<string, int>();
            foreach (PersonAttribute attribute in npc.PersonAttributes.Where(a => a.AttributeType == AttributeType.Role))
            {
                if (!roles.ContainsKey(attribute.Name))
                {
                    roles.Add(attribute.Name, attribute.CurrentValue);
                }
            }

            Dictionary<string, int> skills = new Dictionary<string, int>();
            foreach (PersonAttribute attribute in npc.PersonAttributes.Where(a => a.AttributeType == AttributeType.Skill))
            {
                if (!skills.ContainsKey(attribute.Name))
                {
                    skills.Add(attribute.Name, attribute.CurrentValue);
                }
            }

            Dictionary<string, int> stats = new Dictionary<string, int>();
            foreach (PersonAttribute attribute in npc.PersonAttributes.Where(a => a.AttributeType == AttributeType.Stat))
            {
                if (!stats.ContainsKey(attribute.Name))
                {
                    stats.Add(attribute.Name, attribute.CurrentValue);
                }
            }

            NpcSheetViewModel model = new NpcSheetViewModel
            {
                Person = npc,
                Skills = skills,
                Roles = roles,
                Stats = stats,
                Descriptors = npc.PersonAttributes.Where(a => a.AttributeType == AttributeType.Descriptor).Select(s => s.Name).Distinct().ToList(),
                Wear = npc.PersonAttributes.Where(a=> a.AttributeType == AttributeType.Wear).OrderBy(a => a.CurrentValue).Select(a => a.CurrentValue.ToString()).ToList(),
                Harm = npc.PersonAttributes.Where(a => a.AttributeType == AttributeType.Harm).OrderBy(a => a.CurrentValue).Select(a => a.CurrentValue.ToString()).ToList(),
                Edge = npc.PersonAttributes.Where(a => a.AttributeType == AttributeType.Edge).OrderBy(a => a.CurrentValue).Select(a => a.CurrentValue.ToString()).ToList()
            };
            return model;
        }

        public TrophyNominationViewModel MakeTrophyNominationViewModel(Person person, List<Trophy> trophies, List<AwardNomination> nominations)
        {
            TrophyNominationViewModel model = new TrophyNominationViewModel();
            model.PersonId = person.Id;
            model.Trophies = new SelectList(trophies.OrderBy(t => t.QuickName), "Id", "QuickName");
            model.CanSetAnNemesis = person.IsNpc && person.Awards.All(a => !(a.Trophy.TypeId == (int) TrophyType.NemesisAward && a.NominatedById == CurrentPlayer.Id));
            model.CanSetAsFavourite = person.IsNpc && person.Awards.All(a => !(a.Trophy.TypeId == (int)TrophyType.FirstFavouriteNpc && a.NominatedById == CurrentPlayer.Id));
            model.ExistingNominations = nominations;
            model.IsPrivate = _data.SettingIsEnabled(SettingNames.PrivateAwardNominations); 
            return model;
        }

        private string GetSettingTitle(SettingSection section)
        {
            switch (section)
            {
                case SettingSection.EmailNotifications:
                    return "Email Notification Settings";
                case SettingSection.DailySummaryEmails:
                    return "Summary Emails";
                case SettingSection.ShadowMode:
                    return "Shadow Mode";
                default:
                    return "UNKNONW SETTING SECTION TITLE";
            }
        }

        private UserSettingViewModel Make(Setting setting)
        {
            UserSettingViewModel model = new UserSettingViewModel();

            model.SettingId = setting.Id;
            model.SectionId = setting.SectionId;
            model.Name = setting.DisplayName;
            model.Description = setting.Description;
            model.Enabled = _data.SettingIsEnabled(setting);
            model.EnabledText = setting.TrueText;
            model.DisabledText = setting.FalseText;

            return model;
        }

        private List<MenuItemViewModel> MakePeopleSubmenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();

            if (_data.ShowLeague)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Character League", Url = _urlHelper.Action("CharacterLeague", "Home")
                });
            }

            if (_data.ShowGraveyard)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Graveyard", Url = _urlHelper.Action("Graveyard", "Home")
                });
            }

            if (_data.ShowCharacterSheet)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Download Character Sheet", Url = _urlHelper.Content("~/Content/Documents/character_sheet.docx")
                });
            }

            if (_data.SiteHasFeature(Feature.FavouritesGallery))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Favourites",
                    Url = _urlHelper.Action("FavouritesGallery", "Home")
                });
            }

            if (_data.SiteHasFeature(Feature.SimpleStats) && _data.CurrentUserIsAdmin && _data.SiteHasFeature(Feature.CrowNpcSheet))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "NPC Sheet",
                    Url = _urlHelper.Action("CrowNpcSheet", "Admin")
                });
            }

            return items;
        }

        private List<MenuItemViewModel> MakeUsefulSubmenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();

            if (_data.SiteHasFeature(Feature.Reports))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Statistics",
                    Url = _urlHelper.Action("Index", "Report"),
                });
            }

            items.Add(new MenuItemViewModel
            {
                Name = "Page List",
                Url = _urlHelper.Action("FullPageList", "Home"),
                IconUrl = _urlHelper.Content("~/Content/Images/pages.png")
            });

            if (_data.SiteHasFeature(Feature.WarhammerMap))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Map of the World",
                    Url = "http://www.gitzmansgallery.com/shdmotwow-full.html",
                    IconUrl = _urlHelper.Content("~/Content/Images/globe.png")
                });
            }


            if (_data.SiteHasFeature(Feature.TrophyCabinet))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Trophy Cabinet",
                    Url = _urlHelper.Action("Trophies", "Home"),
                    IconUrl = _urlHelper.Content("~/Content/Images/Trophy.png")
                });
            }

            if (_data.SiteHasFeature(Feature.SessionPage))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Sessions",
                    Url = _urlHelper.Action("Sessions", "Home"),
                    IconUrl = _urlHelper.Content("~/Content/Images/sessions.png")
                });
            }

            if (_data.SiteHasFeature(Feature.CrowRules) && !_data.CurrentUserIsGuest)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Current Game Rules",
                    AltText = "Rules Document",
                    Url = "https://1drv.ms/w/s!AkAJN4vahKOIqhGTXm3ZFcHdIT1C",
                    IconUrl = _urlHelper.Content("~/Content/Images/rules.png"),
                    //  IconCssClass = "badge"
                });
            }

            if (_data.SiteHasFeature(Feature.Bestiary))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Bestiary",
                    Url = _urlHelper.Action("Bestiary", "Home"),
                });
            }

            if (_data.SiteHasFeature(Feature.PublicPrices) && _data.SiteHasFeature(Feature.PriceList))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Price List",
                    Url = _urlHelper.Action("PriceList", "Home"),
                    IconUrl = _urlHelper.Content("~/Content/Images/coin.png")
                });
            }

            items.AddRange(_data.PinnedPages().Select(pinnedPage => new MenuItemViewModel
            {
                Name = pinnedPage.FullName, Url = _urlHelper.Action("Index", "Page", new {id = pinnedPage.Id})
            }));

            return items;
        }
    }
}