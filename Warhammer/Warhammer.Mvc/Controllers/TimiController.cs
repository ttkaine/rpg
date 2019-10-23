using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class EnumDefinition
    {
        public string Area { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    [Authorize(Roles = "Admin")]
    public class TimiController : BaseController
    {
        private readonly IAzureProvider _azure;

        // GET: Timi
        public TimiController(IAuthenticatedDataProvider data, IAzureProvider azure) : base(data)
        {
            _azure = azure;
        }

        public ActionResult MyPeeps()
        {
            return View();
        }

        public ActionResult GetMyPeeps()
        {
            var peeps = DataProvider.OtherPCs().Take(1).ToList();
            return PartialView(peeps);
        }

        public ActionResult Touch()
        {
            List<Page> all = DataProvider.AllPages();
            foreach (Page page in all)
            {
                page.PlainText = page.RawText;
                DataProvider.UpdatePageDetails(page.Id, page.ShortName, page.FullName, page.Description);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult MoveImages()
        {
            List<PageImage> pageImages = DataProvider.AdminGetPageImages().Where(f => string.IsNullOrWhiteSpace(f.FileIdentifier)).ToList();
            foreach (var image in pageImages)
            {
                image.FileIdentifier = _azure.CreateImageBlob(image.Data);
                DataProvider.SaveImage(image);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ClearOldImages()
        {
            List<PageImage> pageImages = DataProvider.AdminGetPageImages()
                .Where(i => i.Data != null)
                .Where(f => !string.IsNullOrWhiteSpace(f.FileIdentifier)).ToList();
            foreach (var image in pageImages)
            {
                image.Data = null;
                DataProvider.SaveImage(image);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}