using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class DefaultXpController : BaseController
    {

        public DefaultXpController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        // GET: DefaultXp
        public ActionResult Index(int? id)
        {
            if (id.HasValue && DataProvider.CurrentPlayerIsGm && !IsEditMode)
            {
                Page page = DataProvider.GetPage(id.Value);

                if (page != null)
                {
                    return PartialView(page);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult AwardXp(int? id)
        {
            if (id.HasValue && DataProvider.CurrentPlayerIsGm)
            {
                Page page = DataProvider.GetPage(id.Value);

                if (page != null)
                {
                    DataProvider.AddDefaultXp(id.Value);
                }
            }
            return RedirectToAction("OutstandingXp", "Gm", new { id });
        }

        [HttpPost]
        public ActionResult RejectXp(int? id)
        {
            if (id.HasValue && DataProvider.CurrentPlayerIsGm)
            {
                Page page = DataProvider.GetPage(id.Value);

                if (page != null)
                {
                    DataProvider.SetPageXpAwarded(page.Id);
                }
            }
            return RedirectToAction("OutstandingXp", "Gm", new { id });
        }
    }
}