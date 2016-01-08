using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;
using Page = Warhammer.Core.Entities.Page;

namespace Warhammer.Mvc.Controllers
{
    public class HomeController : BaseController
    {
        IViewModelFactory _factory;

        private string SiteName
        {
            get
            {
                if (ConfigurationManager.AppSettings["SiteName"] != null)
                {
                    return ConfigurationManager.AppSettings["SiteName"];
                }
                return "Warhammer";
            }
        }

        public HomeController(IAuthenticatedDataProvider data, IViewModelFactory factory) : base(data)
        {
            _factory = factory;
        }

        public ActionResult Index()
        {
            HomePageViewModel model = new HomePageViewModel
            {
                SiteName = SiteName, 
                NewPages = DataProvider.NewPages().OrderByDescending(p => p.SignificantUpdate),
                UpdatedPages = DataProvider.ModifiedPages().OrderByDescending(p => p.SignificantUpdate),
                UpdatedTextSessions = DataProvider.UpdatedTextSessions(),
                MyTurnTextSessions = DataProvider.TextSessionsWhereItisMyTurn(),
                RecentChanges = DataProvider.RecentPages().ToList(),
                MyStuff = DataProvider.MyStuff().ToList(),
                MyPeople = DataProvider.MyPeople().ToList(),
                TopNpcs = DataProvider.TopNpcs(),
                OtherPeople = DataProvider.OtherPCs(),
                NpcWithXp = DataProvider.NpcWithXp(),
                AllPeople = DataProvider.People().Where(m => !DataProvider.MyPeople().Contains(m)).OrderBy(m => m.FullName).ToList()
            };
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

 //       [OutputCache(Duration = 3600, Location = OutputCacheLocation.ServerAndClient, NoStore = true)]
        public ActionResult CharacterLeague()
        {
            List<Person> people = DataProvider.GetLeague();

            return View(people);
        }

        public ActionResult Graveyard()
        {
            List<Person> people = DataProvider.People().Where(p => p.IsDead).OrderBy(s => s.FullName).ToList();
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

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [OutputCache(Duration = 3600, VaryByParam = "id", Location = OutputCacheLocation.ServerAndClient, NoStore = true)]
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
            if (String.IsNullOrWhiteSpace(model.SearchTerm))
            {
                return Search();
            }
            ModelState.Clear();
            List<Page> pages = DataProvider.Search(model.SearchTerm);
            return PartialView(new SearchModel {SearchTerm = model.SearchTerm, Results = pages});
        }

        public ActionResult FavouriteNpcs()
        {
            ModelState.Clear();
            var model = MyFavNpcModel();
            return PartialView(model);
        }

        private MyFavNpcModel MyFavNpcModel()
        {
            List<Person> npcs = DataProvider.AllNpcs().OrderBy(p => p.FullName).ToList();
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
            if (SiteName == "Pirates!")
            {
                return PartialView("OverrideCss", "pirates");
            }
            if (SiteName == "Space Pirates!")
            {
                return PartialView("OverrideCss", "space");
            }
            return null;
        }

        public ActionResult ActiveTextSessions()
        {
            ActiveTextSessionViewModel model = _factory.MakeActiveTextSessionViewModel();

            return PartialView(model);
        }
    }
}