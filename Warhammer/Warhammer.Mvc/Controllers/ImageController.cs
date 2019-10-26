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
            }

            return null;
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
        public async Task<ActionResult> Token(TokenViewModel model)
        {
            string name = _imageData.GetPageName(model.Id);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Unknown";
            }
            string file = _imageData.GetImagefilenameForPage(model.Id);

            ICloudBlob blob = await Task.FromResult(_azure.GetImageBlobReference(file));
            if (blob.Exists())
            {
                blob.FetchAttributes();
                long fileByteLength = blob.Properties.Length;
                Byte[] data = new Byte[fileByteLength];
                blob.DownloadToByteArray(data, 0);

                Image roundImage = _imageProcessor.RoundCorners(data, model.DrawingColor);
                byte[] png = _imageProcessor.GetPngFromImage(roundImage);
                string filename = $"Token_{name.ToAlpha()}.png";
                return File(png, "image/png", $"{filename}");
            }

            return null;
        }
    }
}