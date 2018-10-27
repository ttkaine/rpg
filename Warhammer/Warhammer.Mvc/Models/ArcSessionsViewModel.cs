using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ArcSessionsViewModel
    {
        public bool CanEdit { get; set; }
        public List<SessionListItemViewModel> Sessions { get; set; }

        public ArcSessionsViewModel()
        {
        }
    }
}