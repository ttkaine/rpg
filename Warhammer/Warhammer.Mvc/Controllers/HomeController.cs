using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;
using Page = Warhammer.Core.Entities.Page;

namespace Warhammer.Mvc.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult Index()
        {
            HomePageViewModel model = new HomePageViewModel
            {
                NewPages = DataProvider.NewPages().OrderByDescending(p => p.SignificantUpdate),
                UpdatedPages = DataProvider.ModifiedPages().OrderByDescending(p => p.SignificantUpdate),
                RecentChanges = DataProvider.RecentPages().ToList(),
                MyStuff = DataProvider.MyStuff().ToList(),
                MyPeople = DataProvider.MyPeople().ToList(),
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

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.ServerAndClient, NoStore = true)]
        public ActionResult Trophies()
        {
            List<Trophy> trophies = DataProvider.Trophies().ToList();
            return View(trophies);
        }

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.ServerAndClient, NoStore = true)]
        public ActionResult CharacterLeague()
        {
            List<Person> people = DataProvider.People().OrderByDescending(s => s.PointsValue).ThenByDescending(s => s.Modified).ToList();
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
    }
}