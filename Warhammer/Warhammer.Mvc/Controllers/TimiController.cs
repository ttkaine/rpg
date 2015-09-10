using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class TimiController : BaseController
    {
        // GET: Timi
        public TimiController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult Index()
        {
            List<Page> pages = DataProvider.RecentPages().ToList();
            return View(pages);
        }
    }
}