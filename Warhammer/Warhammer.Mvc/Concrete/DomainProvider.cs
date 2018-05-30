using System.Configuration;
using System.Web;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Concrete
{
    public class DomainProvider : IDomainProvider
    {
        public string CurrentDomain
        {
            get
            {
                string domain = HttpContext.Current.Request.Url.Host;

                if (domain == "localhost")
                {
                    domain = ConfigurationManager.AppSettings["DebugDomain"];
                }
                if (domain == "warhammer.ttk")
                {
                    domain = "warhammer.sendingofeight.co.uk";
                }

                return domain;
            }
        }
    }
}