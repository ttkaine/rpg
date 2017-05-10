namespace Warhammer.Core.Entities
{
    public enum TrophyType
    {
        DefaultAward = 0,
        DeadAward = 1,
        MainPartyBanner = 2,
        FirstFavouriteNpc = 3,
        SecondFavouriteNpc = 4,
        ThirdFavouriteNpc = 5,
        LeadWeightAward = 6

    }

    public partial class Trophy
    {
        public bool SaveAsCurrentCampaignOnly { get; set; }
        public bool CurrentCampaignOnly => CampaignId.HasValue;

        public TrophyType TrophyType
        {
            get { return (TrophyType) TypeId; }
            set { TypeId = (int) value; }
        }

        public string QuickName {
            get
            {
                if (Name.StartsWith("The "))
                {
                    return Name.Substring(4, Name.Length - 4);
                }
                return Name;
            }
        }
    }
}
