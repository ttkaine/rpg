using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Warhammer.Core;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly IImageDataProvider _imageData;

        public ImageController(IImageDataProvider imageData)
        {
            _imageData = imageData;
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        [AllowAnonymous]
        public ActionResult Image(int id)
        {
            PageImage image = _imageData.GetPageImageForPage(id);
            if (image != null)
            {
                return File(image.Data, "image/jpeg");
            }

            var defaultDir = Server.MapPath("/Content/Images");

            var defaultImagePath = Path.Combine(defaultDir, "page-page.png");
            return File(defaultImagePath, "image/jpeg");
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult PageImage(int? id)
        {
            if (id.HasValue)
            {
                string name = _imageData.GetPageName(id.Value);
                ImageViewModel imageViewModel = new ImageViewModel
                {
                    Id = id.Value,
                    Name = name
                };
                return PartialView(imageViewModel);
            }
            return null;
        }
    }
}