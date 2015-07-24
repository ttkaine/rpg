using System.Web.Mvc;

namespace Warhammer.Mvc.Concrete
{
    public class EditModeFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Session != null && filterContext.RequestContext.HttpContext.Session["EditMode"] != null)
            {
                bool editMode = (bool)filterContext.RequestContext.HttpContext.Session["EditMode"];
                filterContext.Controller.ViewBag.EditMode = editMode;
            }
            else
            {
                filterContext.Controller.ViewBag.EditMode = false;
            }
        }
    }
}