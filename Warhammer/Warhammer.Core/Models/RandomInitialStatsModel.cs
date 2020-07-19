using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{

    public enum CoreStat
    {
        Combat = 1,
        Action = 2,
        Work = 3,
        Reason = 4,
        Social = 5,
        Self = 6
    }

    public enum SkillLevel
    {
        Novice = 1,
        Apprentice = 2,
        Professional = 3,
        Master = 4,
        Grandmaster = 5
    }

    public enum AgeBracket
    {
        Child = 1,
        Young = 2,
        MiddleAged = 3,
        Senior = 4,
        Elderly = 5
    }

    

    public class RandomInitialStatsModel
    {
        public int PersonId { get; set; }
        public string PrimaryRole { get; set; }
        public CoreStat? PrimaryStat { get; set; }
        public CoreStat? DumpStat { get; set; }
        public SkillLevel? SkillLevel { get; set; }
        public AgeBracket? AgeBracket { get; set; }
    }
}