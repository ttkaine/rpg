using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class RumourController : BaseController
    {
        // GET: Rumour
        public ActionResult Index()
        {
            if (DataProvider.SiteHasFeature(Feature.RumourMill))
            {
                List<Rumour> rumours = DataProvider.GetAllRumours();

                if (ViewBag.EditMode)
                {
                    rumours.Insert(0, new Rumour());
                }


                return View(rumours);
            }
            return null;
        }

        public ActionResult List()
        {
            if (DataProvider.SiteHasFeature(Feature.RumourMill))
            {
                List<Rumour> rumours = DataProvider.GetAllRumours();

                if (ViewBag.EditMode)
                {
                rumours.Insert(0, new Rumour());
                }
            return PartialView(rumours);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult List(List<Rumour> rumours)
        {
            if (DataProvider.SiteHasFeature(Feature.RumourMill))
            {
                DataProvider.SaveRumours(rumours);

                ModelState.Clear();
                return RedirectToAction("Index");

            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.RumourMill))
            {
                DataProvider.DeleteRumour(id);
                List<Rumour> rumours = DataProvider.GetAllRumours();
                ModelState.Clear();
                return PartialView("List", rumours);
            }
            return null;
        }

        public ActionResult RumoursForPlace(int placeId)
        {
            if (DataProvider.SiteHasFeature(Feature.RumourMill))
            {
                List<Rumour> rumours = DataProvider.GetRumoursForPlace(placeId);
                return PartialView(rumours);
            }
            return null;
        }

        public RumourController(IAuthenticatedDataProvider data) : base(data)
        {
        }
    }
}