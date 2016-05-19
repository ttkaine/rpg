namespace Warhammer.Core.Entities
{
    public enum StatLevel
    {
        Shit = -3,
        Terrible = -2,
        Poor = -1,
        Mediocre = 0,
        Average = 1,
        Fair = 2,
        Good = 3,
        Great = 4,
        Superb = 5,
        Fantastic = 6,
        Epic = 7,
        Legendary = 8,
        Divine = 9
    }
    public enum StatType
    {
        WS = 1,
        BS = 2,
        S = 3,
        T = 4,
        Ag = 5,
        Int = 6,
        WP = 7,
        Fel = 8
    }

    public partial class FateStat
    {
        public string LongDisplayValue
        {
            get
            {
                string name = ((StatLevel) StatValue).ToString();
                if (StatValue >= 0)
                {
                    return string.Format($"+{StatValue} {name}");
                }
                else
                {
                    return string.Format($"{StatValue} {name}");
                }
            }
        }

        public string ShortDisplayValue
        {
            get
            {
                if (StatValue >= 0)
                {
                    return string.Format($"+{StatValue}");
                }
                else
                {
                    return string.Format($"{StatValue}");
                }
            }
        }
    }
}
