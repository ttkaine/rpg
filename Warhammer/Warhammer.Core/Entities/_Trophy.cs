namespace Warhammer.Core.Entities
{
    public enum TrophyType
    {
        DefaultAward = 0,
        DeadAward = 1,
        MainPartyBanner = 2,
        FirstFavouriteNpc = 3,
        SecondFavouriteNpc = 4,
        ThirdFavouriteNpc = 5
        
    }

    public partial class Trophy
    {
        public TrophyType TrophyType
        {
            get { return (TrophyType) TypeId; }
            set { TypeId = (int) value; }
        }
    }
}
