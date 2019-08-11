using System.Collections.Generic;
using System.Linq;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface IPublicDataProvider
    {
        string GetOverrideCssContent();
        bool UserHasAccessToDomain(string username, string domain);
        PageImage GetPageImage(int id);
        SiteIcon GetSiteIcon(int size);
        List<int> GetSiteIconSizes();
        IQueryable<Person> AllPeople();
        List<int> CampaignsWithFeature(Feature feature);
    }
}