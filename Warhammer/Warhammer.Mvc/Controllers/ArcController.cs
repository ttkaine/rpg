using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class ArcController : BaseController
    {
        public ArcController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult GameDateForArc(int id)
        {
            Arc arc = DataProvider.GetArc(id);
            if (arc != null)
            {
                return PartialView(arc);
            }
            return null;
        }

    }
}