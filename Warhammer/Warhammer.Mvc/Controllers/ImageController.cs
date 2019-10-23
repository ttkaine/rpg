using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.WindowsAzure.Storage.Blob;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Core.Models;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        private readonly IImageDataProvider _imageData;
        private readonly IImageProcessor _imageProcessor;
        private readonly IAzureProvider _azure;
        private const string UrlBase = "https://sendingofeight.blob.core.windows.net/images/";

        public ImageController(IImageDataProvider imageData, IImageProcessor imageProcessor, IAzureProvider azure)
        {
            _imageData = imageData;
            _imageProcessor = imageProcessor;
            _azure = azure;
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        [AllowAnonymous]
        public async Task<ActionResult> Image(int id)
        {
            PageImage image = _imageData.GetPageImageForPage(id);
            if (image != null)
            {
                if (!string.IsNullOrWhiteSpace(image.FileIdentifier))
                {
                    ICloudBlob blob = await Task.FromResult(_azure.GetImageBlobReference(image.FileIdentifier));
                    if (blob.Exists())
                    {
                        return File(blob.OpenRead(), "image/jpeg");
                    }
                }

                return File(image.Data, "image/jpeg");
            }

            var defaultDir = Server.MapPath("/Content/Images");

            var defaultImagePath = Path.Combine(defaultDir, "page-page.png");
            return File(defaultImagePath, "image/jpeg");
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult LeagueTrophyImage(int id)
        {
            TrophyImage image = _imageData.GetPageImageForTrophy(id);
            if (image != null)
            {
                return File(image.Data, image.Mime);
            }

            var defaultDir = Server.MapPath("/Content/Images");

            var defaultImagePath = Path.Combine(defaultDir, "page-page.png");
            return File(defaultImagePath, "image/jpeg");
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        [AllowAnonymous]
        public async Task<ActionResult> LeagueImage(int id)
        {
            PageImage image = _imageData.GetPageImageForPage(id, true);
            if (image != null)
            {

                if (!string.IsNullOrWhiteSpace(image.FileIdentifier))
                {
                    ICloudBlob blob = await Task.FromResult(_azure.GetImageBlobReference(image.FileIdentifier));
                    if (blob.Exists())
                    {
                        return File(blob.OpenRead(), "image/jpeg");
                    }
                }

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
                string file = _imageData.GetImagefilenameForPage(id.Value);

                string url = $"{UrlBase}{file}";

           

                ImageViewModel imageViewModel = new ImageViewModel
                {
                    Id = id.Value,
                    Name = name,
                    Url = url
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