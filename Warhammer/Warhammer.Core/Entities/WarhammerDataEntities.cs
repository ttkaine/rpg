using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;

namespace Warhammer.Core.Entities
{
    public partial class WarhammerDataEntities
    {
        private readonly IDomainProvider _domainProvider = DependencyResolver.Current.GetService<IDomainProvider>();

        private int? _campaignId = null;

        public int CurrentCampaignId
        {
            get
            {
                if (!_campaignId.HasValue)
                {
                    _campaignId = CampaignDetails.FirstOrDefault(c => c.Url == _domainProvider.CurrentDomain)?.CampaignId;
                }
                return _campaignId.GetValueOrDefault();
            }
        }

        public bool IsMasterDomain => _domainProvider.IsMasterDomain;

        public override int SaveChanges()
        {
            var addedAuditedEntities = ChangeTracker.Entries()
              .Where(p => p.State == EntityState.Added)
              .Select(p => p.Entity);

            foreach (var added in addedAuditedEntities)
            {
                var property = added.GetType().GetProperty("CampaignId");
                if (property != null)
                {
                    var propvalue = property.GetValue(added);
                    if (propvalue != null)
                    {
                        string propValue = property.GetValue(added).ToString();

                        if (propValue == "0")
                        {
                            property.SetValue(added, CurrentCampaignId, null);
                        }
                    }
                }
            }
            return base.SaveChanges();
        }
    }
}