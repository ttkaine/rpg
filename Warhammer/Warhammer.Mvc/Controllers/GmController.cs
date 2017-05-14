using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.Mvc;
using ASP;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Controllers
{
    public class GmController : BaseController
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly IAuthenticatedUserProvider _user;

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

        public GmController(IAuthenticatedDataProvider data, IImageProcessor imageProcessor, IAuthenticatedUserProvider user) : base(data)
        {
            _imageProcessor = imageProcessor;
            _user = user;
        }

        public ActionResult EditTrophy(int? id)
        {
            Trophy trophy = null;
            if (id.HasValue)
            {
                trophy = DataProvider.GetTrophy(id.Value);
                trophy.SaveAsCurrentCampaignOnly = trophy.CurrentCampaignOnly;
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
            if (ModelState.IsValid && (DataProvider.CurrentPlayerIsGm || _user.IsAdmin))
            {
                ClearTrophyCache(trophy.Id);
                byte[] imageData = null;

                if (imageFile != null)
                {
                    Rectangle cropArea = GetCropArea(y1, x1, h, w);
                    Image theImage = Image.FromStream(imageFile.InputStream, true, true);
                    Image croppedImage = _imageProcessor.Crop(theImage, cropArea);
                    croppedImage = _imageProcessor.ResizeImage(croppedImage, new Size { Height = 200, Width = 200 });
                    imageData = _imageProcessor.GetJpegFromImage(croppedImage);
                }

                if (trophy.Id == 0)
                {
                    DataProvider.AddTrophy(trophy.Name, trophy.Description, trophy.PointsValue, imageData, "image/jpeg", trophy.SaveAsCurrentCampaignOnly);
                }
                else
                {
                    if (imageData != null)
                    {
                        DataProvider.UpdateTrophy(trophy.Id, trophy.Name, trophy.Description, trophy.PointsValue,
                            imageData, "image/jpeg", trophy.SaveAsCurrentCampaignOnly);
                    }
                    else
                    {
                        DataProvider.UpdateTrophy(trophy.Id, trophy.Name, trophy.Description, trophy.PointsValue, trophy.SaveAsCurrentCampaignOnly);
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
    }
}