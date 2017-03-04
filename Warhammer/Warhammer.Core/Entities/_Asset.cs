using Warhammer.Core.Helpers;

namespace Warhammer.Core.Entities
{
    public partial class Asset
    {
        public bool Delete { get; set; }
        public string UpkeepDisplay => CurrencyHelper.DisplayString(Upkeep);
    }
}