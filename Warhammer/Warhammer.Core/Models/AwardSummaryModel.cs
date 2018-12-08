using System;

namespace Warhammer.Core.Models
{
    public class AwardSummaryModel
    {
        public int Id { get; set; }
        public string TrophyName { get; set; }
        public int TrophyId { get; set; }
        public string Reason { get; set; }
        public DateTime AwardedOn { get; set; }
    }
}