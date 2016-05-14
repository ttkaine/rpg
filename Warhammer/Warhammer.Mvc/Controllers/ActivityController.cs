using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Controllers
{
    public class ActivityController : BaseController
    {
        public ActivityController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult RecentActivity()
        {
            List<Object> recentPages = DataProvider.RecentActivity().ToList();
            return PartialView("RecentActivity", recentPages);
        }
    }
}