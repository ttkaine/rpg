using System.Collections.Generic;

namespace Warhammer.Core.Models
{
    public class SessionArcSummaryModel
    {
        public bool SessionHasArc => ArcId.HasValue;
        public int SessionId { get; set; }
        public int? ArcId { get; set; }
        public string Name { get; set; }
        public List<PageListItemModel> Sessions { get; set; }
    }
}