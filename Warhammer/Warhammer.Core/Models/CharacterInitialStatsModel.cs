using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class CharacterInitialStatsModel
    {
        public CharacterInitialStatsModel()
        {
            Stats = new List<StatInitModel>();
        }

        public int PersonId { get; set; }
        public List<StatInitModel> Stats { get; set; }
        public int TotalStats { get; set; }
        public int AverageStat { get; set; }
        public string InitialRoleName { get; set; }
        public string InitialFirstSkillName { get; set; }
        public string InitialSecondSkillName { get; set; }
        public string InitialThirdSkillName { get; set; }
        public string InitialFirstDescriptorName { get; set; }
        public string InitialSecondDescriptorName { get; set; }
        public string InitialThirdDescriptorName { get; set; }
    }

    public class StatInitModel
    {
        public StatName StatName { get; set; }
        public int MinValue { get; set; }
        public int InitialValue { get; set; }
        public int CurrentValue { get; set; }
    }
}