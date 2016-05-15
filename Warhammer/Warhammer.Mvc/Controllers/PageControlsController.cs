using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class PageControlsController : BaseController
    {
        // GET: PageControls
        public ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView(page);
                }
            }
            return null;
        }

        public PageControlsController(IAuthenticatedDataProvider data) : base(data)
        {
        }
    }
}