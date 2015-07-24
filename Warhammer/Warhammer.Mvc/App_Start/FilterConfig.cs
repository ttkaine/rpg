using System.Web.Mvc;
using Warhammer.Mvc.Concrete;

namespace Warhammer.Mvc
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new EditModeFilterAttribute());
        }
    }
}
