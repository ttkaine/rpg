using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class ActiveTextSessionViewModel
    {
        public List<Session> UpdatedTextSessions { get; set; }
        public List<Session> MyTurnTextSessions { get; set; }
    }
}