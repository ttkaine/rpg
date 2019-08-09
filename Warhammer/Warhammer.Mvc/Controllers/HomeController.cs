using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using NUnit.Framework;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Core.Models;
using Warhammer.Mvc.Models;
using Page = Warhammer.Core.Entities.Page;

namespace Warhammer.Mvc.Controllers
{

    public class HomeController : BaseController
    {
        private readonly IAdminSettingsProvider _adminSettings;
        private readonly IPublicDataProvider _publicData;

        private string SiteName
        {
            get
            {
                return _adminSettings.GetAdminSetting(AdminSettingName.SiteName);
            }
        }

        public HomeController(IAuthenticatedDataProvider data, IAdminSettingsProvider adminSettings, IPublicDataProvider publicData) : base(data)
        {
            _adminSettings = adminSettings;
            _publicData = publicData;
        }

        public ActionResult Index()
        {

            HomePageViewModel model = new HomePageViewModel
            {

                SiteName = SiteName, 
                MyPeople = DataProvider.MyPeople().ToList(),
                OtherPeople = DataProvider.OtherPCs(),
            };

            if (!DataProvider.IsMasterDomain)
            {
                model.NewPages = DataProvider.NewPages();
            }

            if (DataProvider.SiteHasFeature(Feature.ShowGameDate))
            {
                CampaignDetail detail = DataProvider.GetCampaginDetails();
                if (detail.CurrentGameDate.HasValue)
                {
                    model.GameDateDisplay =
                        $"Current Game Date: The {detail.CurrentGameDate.Value.ToWarhammerDateString()} ({detail.CurrentGameDate:dddd dd MMMM yyyy})";
                    model.GameDate = detail.CurrentGameDate;
                }
            }

            if (DataProvider.SiteHasFeature(Feature.CharacterLeague))
            {
                model.TopNpcs = DataProvider.TopNpcs();
            }

            return View(model);
        }

        public ActionResult CreatedPages()
        {
            if (DataProvider.IsMasterDomain) { return null; }

            List<PageLinkModel> createdPages = DataProvider.NewPages().ToList();
            return PartialView(createdPages);
        }

        public ActionResult SiteSelector()
        {
            if (!DataProvider.IsMasterDomain) { return null; }

            List<CampaignDetail> campaigns = DataProvider.AllCampaigns();

            return PartialView(campaigns);
        }


        public ActionResult UpdatedPages()
        {
            if (DataProvider.IsMasterDomain) { return null; }

            List<PageLinkModel> updatedPages = DataProvider.ModifiedPages().ToList();
            return PartialView(updatedPages);
        }

        public ActionResult FullPageList()
        {
            List<PageListItemModel> model = DataProvider.FullPageList();
            return View(model);
        }

        public ActionResult Sessions()
        {
            List<SessionListItemViewModel> sessions = DataProvider.AllSessions().OrderByDescending(s => s.DateTime).Select(s => new SessionListItemViewModel(s)).ToList();
            foreach (SessionListItemViewModel session in sessions)
            {
                session.LogButtonPerson = session.People.FirstOrDefault(p => p.PlayerId.HasValue && p.SessionLogs.All(l => l.SessionId != session.Id) && p.Player.UserName == User.Identity.Name);                
            }

            return View(sessions);
        }

        public ActionResult Arcs()
        {
            bool showDates = DataProvider.SiteHasFeature(Feature.ShowGameDate);
            List<ArcListItemViewModel> arcs = 
                DataProvider.Arcs()
                    .OrderByDescending(a => a.CurrentGameDate.Year)
                    .ThenByDescending(a => a.CurrentGameDate.Month)
                    .ThenByDescending(a => a.CurrentGameDate.Day)
                    .Select(a => new ArcListItemViewModel(a, showDates)).ToList();
            return View(arcs);    
        }

        public PartialViewResult PinnedItems()
        {
            List<PageLinkModel> pages = DataProvider.PinnedPages().OrderBy(p => p.FullName).ToList();
            return PartialView(pages);
        }

        public ActionResult Trophies()
        {
            List<Trophy> trophies = DataProvider.Trophies().ToList();
            TrophyCabinetViewModel model = ModelFactory.Make(trophies, DataProvider.CurrentPlayerIsGm, DataProvider.CurrentUserIsAdmin);

            return View(model);
        }

        public ActionResult Trophy(int id)
        {
            Trophy trophy = DataProvider.GetTrophy(id);
            return View(trophy);
        }

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult CharacterLeague()
        {
            CharacterLeagueModel model = DataProvider.GetLeague();
            if (DataProvider.SiteHasFeature(Feature.UpdatedCharacterLeague))
            {
                model.IsSiteVersion = true;
                foreach(var character in model.Items)
                {
                    character.IsSiteVersion = true;
                }
                return View("FullCharacterLeague", model);
            }
            return View(model);
        }

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult FullCharacterLeague()
        {
            if (DataProvider.SiteHasFeature(Feature.FullCharacterLeague))
            {
                CharacterLeagueModel model = DataProvider.GetFullLeague();

                return View(model);
            }

            return RedirectToAction("Index");
        }

        public ActionResult FavouritesGallery()
        {
            if (DataProvider.SiteHasFeature(Feature.FavouritesGallery))
            {
                List<PageLinkModel> favs = DataProvider.GetFavourites();

                return View(favs);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Graveyard()
        {
            List<Person> people = DataProvider.PeopleInGraveyard().ToList();
            return View(people);
        }

        public ActionResult People()
        {
            List<Person> people = DataProvider.People().OrderBy(s => s.FullName).ToList();
            return View(people);
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult TrophyImage(int id)
        {
            Trophy trophy = DataProvider.GetTrophy(id);
            var defaultDir = Server.MapPath("/Content/Images");

            if (trophy != null)
            {
                if (trophy.ImageData != null && trophy.ImageData.Length > 100 && !string.IsNullOrWhiteSpace(trophy.MimeType))
                {
                    return File(trophy.ImageData, trophy.MimeType);
                }
            }

            var defaultImagePath = Path.Combine(defaultDir, "no-image.jpg");

            Response.Cache.SetExpires(DateTime.Now.AddYears(1));
            Response.Cache.SetCacheability(HttpCacheability.Public);

            return File(defaultImagePath, "image/jpeg");
        }

        [HttpGet]
        public ActionResult Search()
        {
            SearchModel model = new SearchModel();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Search(SearchModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                return Search();
            }
            ModelState.Clear();
            List<Page> pages = DataProvider.Search(model.SearchTerm);
            return PartialView(new SearchModel {SearchTerm = model.SearchTerm, Results = pages});
        }

        public ActionResult FavouriteNpcs()
        {
            if (User.IsInRole("Player"))
            {
                ModelState.Clear();
                var model = MyFavNpcModel();
                return PartialView(model);
            }
            return null;
        }

        private MyFavNpcModel MyFavNpcModel()
        {
            List<PageListItemModel> npcs = DataProvider.NpcList();
            MyFavNpcModel model = new MyFavNpcModel
            {
                First = DataProvider.PersonWithMyAward(TrophyType.FirstFavouriteNpc),
                Second = DataProvider.PersonWithMyAward(TrophyType.SecondFavouriteNpc),
                Third = DataProvider.PersonWithMyAward(TrophyType.ThirdFavouriteNpc),
                Nemisis = DataProvider.PersonWithMyAward(TrophyType.NemesisAward),
            };

            model.CanSetFavourites = DataProvider.SiteHasFeature(Feature.FavouritesGallery) &&
                                     !DataProvider.SiteHasFeature(Feature.AwardNominations);

            if (model.First != null)
            {
                model.FirstId = model.First.Id;
                model.ChooseFirstNpcList = new SelectList(npcs, "Id", "FullName", model.First.Id);
            }
            else
            {
                model.ChooseFirstNpcList = new SelectList(npcs, "Id", "FullName");
            }

            if (model.Second != null)
            {
                model.SecondId = model.Second.Id;
                model.ChooseSecondNpcList = new SelectList(npcs, "Id", "FullName", model.Second.Id);
            }
            else
            {
                model.ChooseSecondNpcList = new SelectList(npcs, "Id", "FullName");
            }

            if (model.Third != null)
            {
                model.ThirdId = model.Third.Id;
                model.ChooseThirdNpcList = new SelectList(npcs, "Id", "FullName", model.Third.Id);
            }
            else
            {
                model.ChooseThirdNpcList = new SelectList(npcs, "Id", "FullName");
            }

            if (model.Nemisis != null)
            {
                model.NemisisId = model.Nemisis.Id;
                model.ChooseNemisisNpcList = new SelectList(npcs, "Id", "FullName", model.Nemisis.Id);
            }
            else
            {
                model.ChooseNemisisNpcList = new SelectList(npcs, "Id", "FullName");
            }
            return model;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult UpdateFavNpc(MyFavNpcModel model)
        {
            if (ModelState.IsValid)
            {
                DataProvider.SetMyAward(model.ThirdId, TrophyType.ThirdFavouriteNpc);
                DataProvider.SetMyAward(model.SecondId, TrophyType.SecondFavouriteNpc);
                DataProvider.SetMyAward(model.FirstId, TrophyType.FirstFavouriteNpc);
                if (model.NemisisId > 0)
                {
                    DataProvider.SetMyAward(model.NemisisId, TrophyType.NemesisAward);
                }
            }
            ModelState.Clear();
            var favModel = MyFavNpcModel();
            return PartialView("FavouriteNpcs", favModel);
        }

        [AllowAnonymous]
        public ActionResult OverrideCss()
        {
            string dbCss = _publicData.GetOverrideCssContent();

            if (!string.IsNullOrWhiteSpace(dbCss))
            {
                return PartialView("OverrideCss", dbCss);
            }

            return null;
        }

        public ActionResult ActiveTextSessions()
        {
            if (DataProvider.IsMasterDomain && !DataProvider.SiteHasFeature(Feature.MasterTextSessions))
            {
                 return null;
            }

            ActiveTextSessionViewModel model = ModelFactory.MakeActiveTextSessionViewModel();


            if (Request.IsLocal)
            {                 
                foreach (OpenTextSessionSummaryModel session in model.OpenSessions)
                {
                    session.IsLocal = true;
                }
            }

            return PartialView(model);
        }

        public ActionResult Menu()
        {
            MenuViewModel model = ModelFactory.MakeMenu();


            return PartialView(model);
        }

        public ActionResult Settings()
        {
            UserSettingsViewModel model = ModelFactory.MakeUserSettings();
            return View(model);

        }

        public ActionResult PriceList()
        {
            if (DataProvider.SiteHasFeature(Feature.PriceList))
            {
                if (DataProvider.SiteHasFeature(Feature.PublicPrices) || User.IsInRole("Admin"))
                {
                    List<PriceListItem> priceList = DataProvider.PriceList();
                    if (ViewBag.EditMode)
                    {
                        priceList.Insert(0, new PriceListItem {AllItems = priceList});
                    }
                    return View(priceList);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult PriceList(List<PriceListItem> model)
        {
            if (DataProvider.SiteHasFeature(Feature.PriceList))
            {
                if (DataProvider.SiteHasFeature(Feature.PublicPrices) || User.IsInRole("Admin"))
                {
                    if (ModelState.IsValid)
                    {
                        DataProvider.SavePriceList(model);
                    }
                    ModelState.Clear();
                    List<PriceListItem> priceList = DataProvider.PriceList();
                    if (ViewBag.EditMode)
                    {
                        priceList.Insert(0,new PriceListItem {Id = 0, AllItems = priceList});
                    }
                    return View(priceList);
                }
            }
            return null;
        }

        public ActionResult SettingsSection(int sectionId)
        {
            UserSettingsSectionViewModel model = ModelFactory.Make(DataProvider.SettingSection(sectionId));
            return PartialView(model);
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult SwitchSetting(int settingId)
        {
            int sectionId = DataProvider.SwitchSetting(settingId);
            UserSettingsSectionViewModel model = ModelFactory.Make(DataProvider.SettingSection(sectionId));
            return PartialView("SettingsSection", model);
        }

        public ActionResult AwardHistory()
        {
            if (DataProvider.SiteHasFeature(Feature.AwardHistory))
            {
                List<Award> awards = DataProvider.GetLatestAwards(35);

                if (awards.Any())
                {
                    return PartialView(awards);
                }
            }
            return null;
        }

        [Authorize(Roles = "Player")]
        public ActionResult MarkAllRead()
        {
            List<PageLinkModel> pages = DataProvider.NewPages().ToList();
            foreach (PageLinkModel page in pages)
            {
                DataProvider.MarkAsSeen(page.Id);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Player")]
        public ActionResult MarkAllModifiedRead()
        {
            List<PageLinkModel> pages = DataProvider.ModifiedPages().ToList();
            foreach (PageLinkModel page in pages)
            {
                DataProvider.MarkAsSeen(page.Id);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Footer()
        {
            string footerMessage = DataProvider.VersionInfo();
            return PartialView("Footer", footerMessage);
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult ShowImage(int id)
        {
            PageImage iamge = DataProvider.GetPageImage(id);
            var defaultDir = Server.MapPath("/Content/Images");

            if (iamge != null)
            {
                if (iamge.Data != null && iamge.Data.Length > 100)
                {
                    return File(iamge.Data, "image/jpeg");
                }
            }

            var defaultImagePath = Path.Combine(defaultDir, "no-image.jpg");

            Response.Cache.SetExpires(DateTime.Now.AddYears(1));
            Response.Cache.SetCacheability(HttpCacheability.Public);

            return File(defaultImagePath, "image/jpeg");
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        [AllowAnonymous]
        public ActionResult PublicImage(int id)
        {
            PageImage iamge = _publicData.GetPageImage(id);
            var defaultDir = Server.MapPath("/Content/Images");

            if (iamge != null)
            {
                if (iamge.Data != null && iamge.Data.Length > 100)
                {
                    return File(iamge.Data, "image/jpeg");
                }
            }

            var defaultImagePath = Path.Combine(defaultDir, "no-image.jpg");

            Response.Cache.SetExpires(DateTime.Now.AddYears(1));
            Response.Cache.SetCacheability(HttpCacheability.Public);

            return File(defaultImagePath, "image/jpeg");
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        [AllowAnonymous]
        public ActionResult IconMetaData()
        {
            List<int> icons = _publicData.GetSiteIconSizes();
            return PartialView(icons);
        }


        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        [AllowAnonymous]
        public ActionResult Icon(int id)
        {
            SiteIcon icon = _publicData.GetSiteIcon(id);

            if (icon?.Data != null && icon.Data.Length > 100)
            {
                return File(icon.Data, "image/png");
            }

            return null;
        }

        public ActionResult Bestiary()
        {
            List<Creature> creatures = DataProvider.Creatures().OrderBy(l => l.Breadcrumb).ToList();
            return View(creatures);
        }

        public ActionResult AwardsForTrophy(int id)
        {
            List<Award> awards = DataProvider.AwardsForTrophy(id);
            return PartialView(awards);
        }
    }
}