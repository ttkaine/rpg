namespace Warhammer.Core.Entities
{
    public enum SimpleHitPointType
    {
        Wear = 1,
        Harm = 2
    }

    public enum SimpleHitPointLevel
    {
        Trivial = 1,
        Slight = 2,
        Significant = 4,
        Serious = 6,
        Critical = 8
    }
       

    public partial class SimpleHitPoint
    {
        public SimpleHitPointLevel SimpleHitPointLevel
        {
            get { return (SimpleHitPointLevel)HitPointLevelId; }
            set { HitPointLevelId = (int)value; }
        }

        public SimpleHitPointType SimpleHitPointType
        {
            get { return (SimpleHitPointType)HitPointTypeId; }
            set { HitPointTypeId = (int)value; }
        }

        public string LongDisplayValue
        {
            get
            {
                string name = SimpleHitPointLevel.ToString();
                return string.Format($"+{HitPointLevelId} {name}");
            }
        }
    }
}