using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class EditArcSessionsViewModel
    {
        public int ArcId { get; set; }
        public string ArcTitle { get; set; }
        public List<SessionLinkItemViewModel> CurrentSessions { get; set; }
        public List<SessionLinkItemViewModel> OtherSessions { get; set; }
        public int? AddSessionId { get; set; }

        public EditArcSessionsViewModel()
        {
            CurrentSessions = new List<SessionLinkItemViewModel>();
            OtherSessions = new List<SessionLinkItemViewModel>();
        }
    }
}