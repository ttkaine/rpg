using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class TrophyCabinetViewModel
    {
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanEditGlobal { get; set; }
        public IEnumerable<Trophy> Trophies { get; set; }
    }
}