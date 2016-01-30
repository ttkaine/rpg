using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Concrete
{
    public class ViewModelFactory : IViewModelFactory
    {
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

            List<Session> myOpenTestSessions = _data.MyOpenTextSessions().OrderByDescending(s => s.DateTime).ToList();

            foreach (Session myOpenTestSession in myOpenTestSessions)
            {
                OpenSessionViewModel sessionViewModel = new OpenSessionViewModel {Session = myOpenTestSession, Status = OpenSessionStatus.Stale };

                if (_data.ModifiedTextSessions().Contains(myOpenTestSession))
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

            return model;
        }

        public PersonStatViewModel MakeStatModel(Person person)
        {
            PersonStatViewModel model = new PersonStatViewModel {PersonId = person.Id, CharacterName = person.ShortName };

            model.Stats = person.Stats;
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

            return model;

        }

        public MenuViewModel MakeMenu()
        {
            MenuViewModel model = new MenuViewModel();

            List<MenuItemViewModel> usefulSubMenu = MakeUsefulSubmenu();
            List<MenuItemViewModel> peopleSubMenu = MakePeopleSubmenu();

            model.LeftMenu.Add(new MenuItemViewModel
            {
                Name = "Useful Pages",
                Url = "#",
                SubMenu = usefulSubMenu
            });

            model.LeftMenu.Add(new MenuItemViewModel
            {
                Name = "People",
                Url = "#",
                SubMenu = peopleSubMenu
            });

            if (_data.SiteHasFeature(Feature.UserSettings))
            {
                model.RightMenu.Add(new MenuItemViewModel
                {
                    Name = "",
                    AltText = "Settings",
                    Url = _urlHelper.Action("Settings", "Home"),
                    IconUrl = _urlHelper.Content("~/Content/Images/Settings.png")
                });
            }

            return model;
        }

        private List<MenuItemViewModel> MakePeopleSubmenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();

            if (_data.CurrentUserIsAdmin)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Browse...",
                    Url = _urlHelper.Action("People", "Home")
                });
            }


            if (_data.ShowLeague)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Character League",
                    Url = _urlHelper.Action("CharacterLeague", "Home")
                });
            }

            if (_data.ShowGraveyard)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Graveyard",
                    Url = _urlHelper.Action("Graveyard", "Home")
                });
            }

            if (_data.ShowCharacterSheet)
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Download Character Sheet",
                    Url = _urlHelper.Content("~/Content/Documents/character_sheet.docx")
                });
            }

            return items;

        }

        private List<MenuItemViewModel> MakeUsefulSubmenu()
        {
            List<MenuItemViewModel> items = new List<MenuItemViewModel>();

            if (_data.SiteHasFeature(Feature.WarhammerMap))
            {
                items.Add(new MenuItemViewModel
                {
                    Name = "Map of the World",
                    Url = "http://www.gitzmansgallery.com/shdmotwow-full.html",
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
                });
            }

            items.AddRange(_data.PinnedPages().Select(pinnedPage => new MenuItemViewModel
            {
                Name = pinnedPage.FullName,
                Url = _urlHelper.Action("Index", "Page", new { id = pinnedPage.Id })
            }));


            return items;
        }
    }
}