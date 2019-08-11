namespace Warhammer.Core.Models
{
    public class PlayerCampaignLinkModel
    {
        public int PlayerId { get; set; }
        public int PlayerCampaignId { get; set; }
        public bool IncludeInCampaign { get; set; }
        public string PlayerName { get; set; }
        public string PlayerEmail { get; set; }
        public bool ShowInGlobal { get; set; }
    }
}