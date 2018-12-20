using System;
using System.Collections.Generic;
using Warhammer.Core.Models;

namespace Warhammer.Mvc.Models
{
    public class ActiveTextSessionViewModel
    {
        public ActiveTextSessionViewModel()
        {
            OpenSessions = new List<OpenTextSessionSummaryModel>();
        }

        public List<OpenTextSessionSummaryModel> OpenSessions { get; set; }
    }
}