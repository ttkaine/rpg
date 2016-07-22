using System.Web;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Concrete
{
    public class AuthenticatedUserProvider : IAuthenticatedUserProvider
    {
        public bool UserIsAuthenticated
        {
            get
            {
                var context = HttpContext.Current;
                if (context != null && context.User != null && context.User.Identity != null)
                    return HttpContext.Current.User.Identity.IsAuthenticated;
                else
                    return false;
            }
        }

        public string UserName
        {
            get
            {
                var context = HttpContext.Current;
                if (context != null && context.User != null && context.User.Identity != null)
                    return HttpContext.Current.User.Identity.Name;
                else
                    return null;
            }
        }
        public bool IsAdmin
        {
            get
            {
                var context = HttpContext.Current;
                if (context != null && context.User != null && context.User.Identity != null)
                    return HttpContext.Current.User.IsInRole("Admin");
                else
                    return false;
            }
        }
    }
}