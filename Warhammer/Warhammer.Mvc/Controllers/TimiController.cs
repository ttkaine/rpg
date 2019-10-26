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
    }
}