using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class SessionLinkItemViewModel
    {
        public int SessionId { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }

        public bool IsInArc { get; set; }
        public string CurrentArcName { get; set; }

        public SessionLinkItemViewModel()
        {            
        }

        public SessionLinkItemViewModel(Session session)
        {
            SessionId = session.Id;
            ShortName = session.ShortName;
            FullName = session.FullName;
            Description = session.Description;

            IsInArc = session.Arc != null;
            if (IsInArc)
            {
                CurrentArcName = session.Arc?.ShortName ?? "Unknown Arc";
            }
        }

    }
}