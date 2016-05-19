using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warhammer.Mvc.Models
{
    public class FateStuntsViewModel
    {
        public List<FateStuntViewModel> StuntModels { get; set; } 
        public int PersonId { get; set; }
        public bool CanEdit { get; set; }
    }
}