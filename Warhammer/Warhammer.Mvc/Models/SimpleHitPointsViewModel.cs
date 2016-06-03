using System.Collections.Generic;

namespace Warhammer.Mvc.Models
{
    public class SimpleHitPointsViewModel
    {
        public List<HitPointViewModel> HarmTrack { get; set; }
        public List<HitPointViewModel> WearTrack { get; set; }
    }

    public class HitPointViewModel
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool CanBuy { get; set; }
        public int CostToBuy { get; set; }
    }
}