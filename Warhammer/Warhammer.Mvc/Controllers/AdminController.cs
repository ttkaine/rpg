using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IDatabaseUpdateProvider _databaseUpdate;
        private readonly IImageProcessor _imageProcessor;
        private readonly IAdminSettingsProvider _adminSettings;
        // GET: Admin
        public AdminController(IAuthenticatedDataProvider data, IDatabaseUpdateProvider databaseUpdate, IImageProcessor imageProcessor, IAdminSettingsProvider adminSettings) : base(data)
        {
            _databaseUpdate = databaseUpdate;
            _imageProcessor = imageProcessor;
            _adminSettings = adminSettings;
        }


        public ActionResult DbUpdate()
        {
          //  string folder = Server.MapPath(Url.Content("~/Content/DbUpdateScripts/"));

            string folder = Server.MapPath(Url.Content("~/Content/DbUpdateScripts/"));
            string backupPath = ConfigurationManager.AppSettings["DatabaseBackupLocation"];
            DatabaseUpdateResult result = _databaseUpdate.PerformUpdates(folder, backupPath);

            return View(result);
        }

        public ActionResult EditTrophy(int? id)
        {
            Trophy trophy = null;
            if (id.HasValue)
            {
                trophy = DataProvider.GetTrophy(id.Value);    
            }

            if (trophy == null)
            {
                trophy = new Trophy();
            }

            return View(trophy); 
        }
        
        [HttpPost]
        public ActionResult EditTrophy(Trophy trophy, HttpPostedFileBase imageFile, double? y1, double? x1, double? h, double? w)
        {
            if (ModelState.IsValid)
            {
                ClearTrophyCache(trophy.Id);
                byte[] imageData = null;

                if (imageFile != null)
                {
                    Rectangle cropArea = GetCropArea(y1, x1, h, w);
                    Image theImage = System.Drawing.Image.FromStream(imageFile.InputStream, true, true);
                    Image croppedImage = _imageProcessor.Crop(theImage, cropArea);
                    croppedImage = _imageProcessor.ResizeImage(croppedImage, new Size { Height = 200, Width = 200 });
                    imageData = _imageProcessor.GetJpegFromImage(croppedImage);
                }

                if (trophy.Id == 0)
                {
                    DataProvider.AddTrophy(trophy.Name, trophy.Description, trophy.PointsValue, imageData,"image/jpeg");
                }
                else
                {
                    if (imageData != null)
                    {
                        DataProvider.UpdateTrophy(trophy.Id, trophy.Name, trophy.Description, trophy.PointsValue,
                            imageData, "image/jpeg");
                    }
                    else
                    {
                        DataProvider.UpdateTrophy(trophy.Id, trophy.Name, trophy.Description, trophy.PointsValue);                      
                    }
                }
                return RedirectToAction("Trophies", "Home");
            }


            return View(trophy);
        }

        private void ClearTrophyCache(int id)
        {
            string path = Url.Action("TrophyImage", "Home", new { id });

            if (path != null)
            {
                Response.RemoveOutputCacheItem(path);
            }

            path = Url.Action("Trophies", "Home", new { id });

            if (path != null)
            {
                Response.RemoveOutputCacheItem(path);
            }
        }

        private Rectangle GetCropArea(double? y1, double? x1, double? h, double? w)
        {
            if (y1.HasValue && x1.HasValue && h.HasValue && w.HasValue)
            {
                return new Rectangle((int)x1.Value, (int)y1.Value, (int)w.Value, (int)h.Value);
            }

            return new Rectangle();
        }


        public ActionResult ManageAwards(int id)
        {
            Core.Entities.Person person = DataProvider.People().FirstOrDefault(p => p.Id == id);
            List<Trophy> trophies = DataProvider.Trophies().ToList();
            if (person == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ManageAwardsViewModel model = new ManageAwardsViewModel { Trophies = new SelectList(trophies, "Id", "Name"), Person = person };
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
                DataProvider.AwardTrophy(model.Person.Id, model.SelectedTrophy, model.Reason);
                return RedirectToAction("ManageAwards", "Admin", new { id = model.Person.Id });
            }
            else
            {
                Core.Entities.Person person = DataProvider.People().FirstOrDefault(p => p.Id == model.Person.Id);
                List<Trophy> trophies = DataProvider.Trophies().ToList();
                model.Trophies = new SelectList(trophies, "Id", "Name");
                model.Person = person;
                return View(model);
            }
        }

        public ActionResult KillPerson(int id)
        {
            Core.Entities.Person person = DataProvider.People().FirstOrDefault(p => p.Id == id);
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




        public ActionResult DeletePage(int id)
        {
            Core.Entities.Page page = DataProvider.GetPage(id);
            if (page == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(page);
            }
        }

        [HttpPost]
        public ActionResult DeletePage(Core.Entities.Page page)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    DataProvider.DeletePage(page.Id);
                }
                catch (Exception ex)
                {
                    return View("DeleteError", ex);
                }
                
            }
            return RedirectToAction("Index", "Home");
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
    }
}