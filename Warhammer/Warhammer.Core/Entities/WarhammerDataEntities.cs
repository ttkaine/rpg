using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;

namespace Warhammer.Core.Entities
{
    public partial class WarhammerDataEntities
    {
        private readonly ICurrentCampaignProvider _campaignProvider =
            DependencyResolver.Current.GetService<ICurrentCampaignProvider>();

        private int? _campaignId = null;

        private int CurrentCampaignId
        {
            get
            {
                if (!_campaignId.HasValue)
                {
                    _campaignId = _campaignProvider.CurrentCampaignId;
                }
                return _campaignId.Value;
            }
        }

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
                    string propValue = property.GetValue(added).ToString();

                    if (propValue == "0")
                    {
                        property.SetValue(added, CurrentCampaignId, null);
                    }
                }
            }
            return base.SaveChanges();
        }
    }
}