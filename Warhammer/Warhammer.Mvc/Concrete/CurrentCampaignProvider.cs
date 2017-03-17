using System.Configuration;
using System.Web;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Concrete
{
    public class CurrentCampaignProvider : ICurrentCampaignProvider
    {
        public int CurrentCampaignId
        {
            get
            {
                string domain = HttpContext.Current.Request.Url.Host;

                if (domain == "localhost")
                {
                    domain = ConfigurationManager.AppSettings["DebugDomain"];
                }

                if (domain == "warhammer.sendingofeight.co.uk")
                {
                    return 1;
                }

                if (domain == "crowhammer.sendingofeight.co.uk")
                {
                    return 1;
                }

                if (domain == "fatehammer.sendingofeight.co.uk")
                {
                    return 2;
                }

                if (domain == "darknet.sendingofeight.co.uk")
                {
                    return 3;
                }

                return 0;

            }
        }
    }
}