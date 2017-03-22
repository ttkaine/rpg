using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
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
        private readonly IScoreCalculator _scoreCalculator;
        private readonly IAdminSettingsProvider _adminSettings;

        private string SiteName
        {
            get
            {
                return _adminSettings.GetAdminSetting(AdminSettingName.SiteName);
            }
        }

        private string CssOverride
        {
            get
            {
                return _adminSettings.GetAdminSetting(AdminSettingName.CssOverrideFilename);
            }
        }

        public HomeController(IAuthenticatedDataProvider data, IScoreCalculator scoreCalculator, IAdminSettingsProvider adminSettings) : base(data)
        {
            _scoreCalculator = scoreCalculator;
            _adminSettings = adminSettings;
        }

        public ActionResult Index()
        {

            HomePageViewModel model = new HomePageViewModel
            {

                SiteName = SiteName, 
                NewPages = DataProvider.NewPages().OrderByDescending(p => p.SignificantUpdate),
                UpdatedPages = DataProvider.ModifiedPages().OrderByDescending(p => p.SignificantUpdate),
                MyPeople = DataProvider.MyPeople().ToList(),
                OtherPeople = DataProvider.OtherPCs(),
            };

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


            if (DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                model.NpcWithXp = DataProvider.NpcWithXp();
            }

            if (DataProvider.SiteHasFeature(Feature.CharacterLeague))
            {
                model.TopNpcs = DataProvider.TopNpcs();
            }

            return View(model);
        }

        public ActionResult FullPageList()
        {
            List<PageListItemModel> model = DataProvider.FullPageList();
            return View(model);
        }

        public ActionResult Sessions()
        {
            List<Session> sessions = DataProvider.Sessions().OrderByDescending(s => s.DateTime).ToList();
            return View(sessions);
        }

        public PartialViewResult PinnedItems()
        {
            List<Page> pages = DataProvider.PinnedPages().OrderBy(p => p.FullName).ToList();
            return PartialView(pages);
        }

        public ActionResult Trophies()
        {
            List<Trophy> trophies = DataProvider.Trophies().ToList();
            return View(trophies);
        }

        public ActionResult Trophy(int id)
        {
            Trophy trophy = DataProvider.GetTrophy(id);
            return View(trophy);
        }

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult CharacterLeague()
        {
            List<Person> people = DataProvider.GetLeague();

            return View(people);
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
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
                Third = DataProvider.PersonWithMyAward(TrophyType.ThirdFavouriteNpc)
            };

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
            }
            ModelState.Clear();
            var favModel = MyFavNpcModel();
            return PartialView("FavouriteNpcs", favModel);
        }

        [AllowAnonymous]
        public ActionResult OverrideCss()
        {
            //this probably shouldn't be here - but, hey, I'll put it someplace better when I get round to it... okay?
            _scoreCalculator.UpdateScores();


            if (!string.IsNullOrWhiteSpace(CssOverride))
            {
                return PartialView("OverrideCss", CssOverride);
            }
            return null;
        }

        public ActionResult ActiveTextSessions()
        {
            ActiveTextSessionViewModel model = ModelFactory.MakeActiveTextSessionViewModel();

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
            List<Page> pages = DataProvider.NewPages().OrderByDescending(p => p.SignificantUpdate).ToList();
            foreach (Page page in pages)
            {
                DataProvider.MarkAsSeen(page.Id);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Player")]
        public ActionResult MarkAllModifiedRead()
        {
            List<Page> pages = DataProvider.ModifiedPages().OrderByDescending(p => p.SignificantUpdate).ToList();
            foreach (Page page in pages)
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

        public ActionResult Bestiary()
        {
            List<Creature> creatures = DataProvider.Creatures().OrderBy(l => l.Breadcrumb).ToList();
            return View(creatures);
        }
    }
}