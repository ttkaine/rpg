using System.Collections.Generic;

namespace Warhammer.Core.Entities
{
    public partial class Award
    {
        public bool Remove { get; set; }
        public List<Session> PossibleSessions { get; set; }
    }
}