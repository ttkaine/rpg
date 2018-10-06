using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IDatabaseUpdateProvider _databaseUpdate;
        private readonly IImageProcessor _imageProcessor;
        private readonly IAdminSettingsProvider _adminSettings;
        private readonly IDomainProvider _domain;
        // GET: Admin
        public AdminController(IAuthenticatedDataProvider data, IDatabaseUpdateProvider databaseUpdate, IImageProcessor imageProcessor, IAdminSettingsProvider adminSettings, IDomainProvider domain) : base(data)
        {
            _databaseUpdate = databaseUpdate;
            _imageProcessor = imageProcessor;
            _adminSettings = adminSettings;
            _domain = domain;
        }


        public ActionResult DbUpdate()
        {
          //  string folder = Server.MapPath(Url.Content("~/Content/DbUpdateScripts/"));

            string folder = Server.MapPath(Url.Content("~/Content/DbUpdateScripts/"));
            string backupPath = ConfigurationManager.AppSettings["DatabaseBackupLocation"];
            DatabaseUpdateResult result = _databaseUpdate.PerformUpdates(folder, backupPath);

            return View(result);
        }

        public ActionResult ManageAwards(int id)
        {
            Person person = DataProvider.GetPerson(id);
            List<Trophy> trophies = DataProvider.Trophies().ToList();
            if (person == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ManageAwardsViewModel model = new ManageAwardsViewModel { Trophies = new SelectList(trophies, "Id", "Name"), Awards = person.Awards.ToList() };
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult RemoveAward(int personId, int awardId)
        {
            DataProvider.RemoveAward(personId, awardId);
            return RedirectToAction("ManageAwards", "Admin", new { id = personId });

        }

        [HttpPost]
        public ActionResult ManageAwards(ManageAwardsViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.SelectedTrophy.HasValue)
                {
                    DataProvider.AwardTrophy(model.PersonId, model.SelectedTrophy.Value, model.Reason);
                    return RedirectToAction("ManageAwards", "Admin", new {id = model.PersonId});
                }
            }

            Person person = DataProvider.GetPerson(model.PersonId);
            List<Trophy> trophies = DataProvider.Trophies().ToList();
            model.Trophies = new SelectList(trophies, "Id", "Name");
            model.Awards = person.Awards.ToList();
            return View(model);

        }

        public ActionResult KillPerson(int id)
        {
            Core.Entities.Person person = DataProvider.GetPerson(id);
            if (person == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(person);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult KillPerson(Person person)
        {
            if (ModelState.IsValid)
            {
                if (person.IsDead)
                {
                    DataProvider.ResurrectPerson(person.Id);
                    return RedirectToAction("Index", "Page", new { id = person.Id });
                }
                else
                {
                    DataProvider.KillPerson(person.Id, person.Obiturary, person.CauseOfDeath);
                    return RedirectToAction("Graveyard", "Home");
                }
                    
            }
            return View(person);
        }






        public ViewResult Log()
        {
            List<ExceptionLog> logs = DataProvider.GetExceptionLogs(50);
            return View(logs);
        }


        public ViewResult Features()
        {
            DataProvider.EnsureFeatures();   
            return View();
        }

        public ActionResult FeatureList()
        {
            List<SiteFeature> features = DataProvider.AllFeatures();
            return PartialView(features);
        }

        [HttpPost]
        public ActionResult DisableFeature(string featureName)
        {
            DataProvider.DisableFeature(featureName);
            List<SiteFeature> features = DataProvider.AllFeatures();
            return PartialView("FeatureList",features);
        }

        [HttpPost]
        public ActionResult EnableFeature(string featureName)
        {
            DataProvider.EnableFeature(featureName);
            List<SiteFeature> features = DataProvider.AllFeatures();
            return PartialView("FeatureList", features);
        }

        public ActionResult AdminSettings()
        {
            List<AdminSetting> settings = _adminSettings.AdminSettings();
            return View(settings);
        }

        [HttpPost]
        public ActionResult AdminSettings(IEnumerable<AdminSetting> model)
        {
            foreach (AdminSetting adminSetting in model)
            {
                _adminSettings.SetAdminSettingValue(adminSetting.Name, adminSetting.SettingValue);
            }
            return RedirectToAction("Features");
        }

        public ActionResult OutstandingXp()
        {
            List<Page> pages = DataProvider.PagesWithOutstandingXp();
            return View(pages);
        }

        public ActionResult XpToSpend()
        {
            List<PageLinkModel> pages = DataProvider.PeopleWithXpToSpend();
            return View(pages);
        }

        public ActionResult CrowNpcSheet()
        {
            if (DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                List<Person> npcs = DataProvider.NpcsWithStats();
                return View(npcs);
            }
            return null;
        }

        public ActionResult CampaignSettings()
        {
            CampaignDetail details = DataProvider.GetCampaginDetails();
            return View(details);
        }

        

        public ActionResult SetGameDate(CampaignDetail model)
        {
            if (ModelState.IsValid)
            {
                DataProvider.SetGameDate(model.CurrentGameDate);
            }

            return RedirectToAction("CampaignSettings");
        }

        public ActionResult AddDayToGameDate()
        {
            if (ModelState.IsValid)
            {
                DataProvider.AddDayToGameDate();
            }

            return RedirectToAction("CampaignSettings");
        }

        public ActionResult AddWeekToGameDate()
        {
            if (ModelState.IsValid)
            {
                DataProvider.AddWeekToGameDate();
            }

            return RedirectToAction("CampaignSettings");
        }

        public ActionResult AddMonthToGameDate()
        {
            if (ModelState.IsValid)
            {
                DataProvider.AddMonthToGameDate();
            }

            return RedirectToAction("CampaignSettings");
        }

        [HttpPost]
        public ActionResult SetCss(string customCss)
        {
            if (ModelState.IsValid)
            {
                DataProvider.SetCustomCss(customCss);
            }

            return RedirectToAction("CampaignSettings");
        }

        public ActionResult CreateCampaign()
        {
            if (DataProvider.CurrentCampaignId > 0)
            {
               return RedirectToAction("AdminSettings", "Admin");
            }
            CampaignDetail details = new CampaignDetail {AvailableGms = DataProvider.GetAllPlayers()};
            return View(details);
        }

        [HttpPost]
        public ActionResult CreateCampaign(CampaignDetail model)
        {
            if (ModelState.IsValid)
            {
                DataProvider.CreateCampaign(_domain.CurrentDomain, model.CustomCss, model.CurrentGameDate, model.GmId);
               return RedirectToAction("AdminSettings", "Admin");
            }
            model.AvailableGms = DataProvider.GetAllPlayers();
            return View(model);
        }

        public ActionResult SetCampaignName(string name)
        {
            if (ModelState.IsValid)
            {
                DataProvider.SetCampaignName(name);
            }

            return RedirectToAction("CampaignSettings");
        }
    }
}