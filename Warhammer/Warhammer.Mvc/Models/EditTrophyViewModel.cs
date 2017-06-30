using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class EditTrophyViewModel
    {
        public bool CurrentCampaignOnly { get; set; }
        public bool CanEdit { get; set; }
        public bool CanEditGlobal { get; set; }
        public Trophy Trophy { get; set; }
    }
}