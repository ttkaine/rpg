using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class AdminPageControlsController : BaseController
    {
        public AdminPageControlsController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult Index(int? id)
        {
            if (id.HasValue && User.IsInRole("Admin"))
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView(page);
                }
            }
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ToggleTextSession(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.ToggleSetAsTextSession(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult OpenCloseTextSession(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.OpenOrCloseTextSession(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult PinPage(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.PinPage(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ToggleSessionPrivicy(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.ToggleSessionPrivacy(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }
    }
}