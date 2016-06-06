using System.Collections.Generic;

namespace Warhammer.Mvc.Models
{
    public class SimpleHitPointsViewModel
    {
        public int PersonId { get; set; }
        public List<HitPointViewModel> HarmTrack { get; set; }
        public List<HitPointViewModel> WearTrack { get; set; }
        public bool CanEdit { get; set; }
    }

    public class HitPointViewModel
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool CanBuy { get; set; }
        public int CostToBuy { get; set; }
        public int PersonId { get; set; }
        public int Type { get; set; }
        public int Level { get; set; }
    }
}