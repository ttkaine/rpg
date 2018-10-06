using System.Collections.Generic;

namespace Warhammer.Core.Entities
{
    public partial class CampaignDetail
    {
        public List<Player> AvailableGms { get; set; }
        public string DisplayName {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    return Name;
                }
                return Url;
            } }
    }
}