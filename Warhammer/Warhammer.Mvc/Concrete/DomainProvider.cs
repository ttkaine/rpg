using System.Configuration;
using System.Web;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Concrete
{
    public class DomainProvider : IDomainProvider
    {
        public bool IsMasterDomain => CurrentDomain == "global.sendingofeight.co.uk";

        public string CurrentDomain
        {
            get
            {
                string domain = HttpContext.Current.Request.Url.Host;

                if (domain == "localhost" || domain == "warhammer.local")
                {
                    domain = ConfigurationManager.AppSettings["DebugDomain"];
                }
                if (domain == "warhammer.ttk")
                {
                    domain = "warhammer.sendingofeight.co.uk";
                }

                if (domain.StartsWith("local."))
                {
                    domain = domain.Substring(6, domain.Length - 6);
                }

                return domain;
            }
        }
    }
}