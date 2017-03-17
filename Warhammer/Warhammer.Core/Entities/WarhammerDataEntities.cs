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
                    property.SetValue(added, _campaignProvider.CurrentCampaignId, null);
                }
            }
            return base.SaveChanges();
        }
    }
}