using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly IImageDataProvider _imageData;
        private readonly IImageProcessor _imageProcessor;

        public ImageController(IImageDataProvider imageData, IImageProcessor imageProcessor)
        {
            _imageData = imageData;
            _imageProcessor = imageProcessor;
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
        [AllowAnonymous]
        public ActionResult LeagueImage(int id)
        {
            PageImage image = _imageData.GetPageImageForPage(id, true);
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

        [HttpPost]
        public ActionResult Token(TokenViewModel model)
        {
            PageImage image = _imageData.GetPageImageForPage(model.Id);
            if (image != null)
            {
                Image roundImage = _imageProcessor.RoundCorners(image.Data, model.DrawingColor);
                byte[] png = _imageProcessor.GetPngFromImage(roundImage);
                string name = _imageData.GetPageName(model.Id);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = "Unknown";
                }
                string filename = $"Token_{name.ToAlpha()}.png";
                return File(png, "image/png", $"{filename}");
            }

            var defaultDir = Server.MapPath("/Content/Images");

            var defaultImagePath = Path.Combine(defaultDir, "page-page.png");
            return File(defaultImagePath, "image/jpeg");
        }
    }
}