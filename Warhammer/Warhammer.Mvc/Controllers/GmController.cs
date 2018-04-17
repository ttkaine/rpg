using System;
using System.Collections.Generic;
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
    public class GmController : BaseController
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly IAuthenticatedUserProvider _user;
        private readonly ICurrentCampaignProvider _campaignProvider;

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
        public GmController(IAuthenticatedDataProvider data, IImageProcessor imageProcessor, IAuthenticatedUserProvider user, ICurrentCampaignProvider campaignProvider) : base(data)
        {
            _imageProcessor = imageProcessor;
            _user = user;
            _campaignProvider = campaignProvider;
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
                trophy = new Trophy {CampaignId = _campaignProvider.CurrentCampaignId };
            }

            EditTrophyViewModel model = ModelFactory.Make(trophy, DataProvider.CurrentPlayerIsGm, DataProvider.CurrentUserIsAdmin);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditTrophy(EditTrophyViewModel trophyModel, HttpPostedFileBase imageFile, double? y1, double? x1, double? h, double? w)
        {
            if (ModelState.IsValid && (DataProvider.CurrentPlayerIsGm || _user.IsAdmin))
            {
                ClearTrophyCache(trophyModel.Trophy.Id);
                byte[] imageData = null;

                if (imageFile != null)
                {
                    Rectangle cropArea = GetCropArea(y1, x1, h, w);
                    Image theImage = Image.FromStream(imageFile.InputStream, true, true);
                    Image croppedImage = _imageProcessor.Crop(theImage, cropArea);
                    croppedImage = _imageProcessor.ResizeImage(croppedImage, new Size { Height = 200, Width = 200 });
                    imageData = _imageProcessor.GetJpegFromImage(croppedImage);
                }

                if (trophyModel.Trophy.Id == 0)
                {
                    DataProvider.AddTrophy(trophyModel.Trophy.Name, trophyModel.Trophy.Description, trophyModel.Trophy.PointsValue, imageData, "image/jpeg", trophyModel.CurrentCampaignOnly);
                }
                else
                {
                    if (imageData != null)
                    {
                        DataProvider.UpdateTrophy(trophyModel.Trophy.Id, trophyModel.Trophy.Name, trophyModel.Trophy.Description, trophyModel.Trophy.PointsValue,
                            imageData, "image/jpeg", trophyModel.CurrentCampaignOnly);
                    }
                    else
                    {
                        DataProvider.UpdateTrophy(trophyModel.Trophy.Id, trophyModel.Trophy.Name, trophyModel.Trophy.Description, trophyModel.Trophy.PointsValue, trophyModel.CurrentCampaignOnly);
                    }
                }
                return RedirectToAction("Trophies", "Home");
            }

            EditTrophyViewModel model = ModelFactory.Make(trophyModel.Trophy, DataProvider.CurrentPlayerIsGm, DataProvider.CurrentUserIsAdmin);
            return View(model);
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

        public ActionResult NpcSheet()
        {
            if (DataProvider.CurrentPlayerIsGm || _user.IsAdmin)
            {
                List<Person> npcs = DataProvider.GetNpcSheetPeople();
                List<NpcSheetViewModel> models = npcs.Select(n => ModelFactory.MakeNpcSheetViewModel(n)).ToList();
                return View(models);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult PersonWithTrophyCharacterSheets(int id)
        {
            if (DataProvider.CurrentPlayerIsGm || _user.IsAdmin)
            {
                List<Person> people = DataProvider.GetCharacterSheetPeopleWithTrophy(id);
                List<NpcSheetViewModel> models = people.Select(n => ModelFactory.MakeNpcSheetViewModel(n)).ToList();
                return View("NpcSheet", models);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult OutstandingAwardNominations()
        {
            List<AwardNomination> nominations = DataProvider.OutstandingNominations();
            return View(nominations);
        }

        [HttpPost]
        public ActionResult AcceptAwardNomination(AwardNomination nomination)
        {
            if (ModelState.IsValid)
            {
                DataProvider.AcceptNomination(nomination.Id, nomination.AcceptedReason, nomination.NominationReason);
            }
            return RedirectToAction("OutstandingAwardNominations");
        }

        [HttpPost]
        public ActionResult RejectAwardNomination(AwardNomination nomination)
        {
            if (ModelState.IsValid)
            {
                DataProvider.RejectNomination(nomination.Id, nomination.RejectedReason);
            }
            return RedirectToAction("OutstandingAwardNominations");
        }

        public ActionResult GmToolsForTrophy(int id)
        {
            if (DataProvider.CurrentPlayerIsGm || _user.IsAdmin)
            {
                return PartialView(id);
            }
            return null;
        }
    }


}