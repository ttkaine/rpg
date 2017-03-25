using System;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class PersonAttributeAdvanceModel
    {
        public decimal CurrentXp => CharacterInfo.CurrentXp;
        public int TotalAdvancesTaken => CharacterInfo.TotalAdvancesTaken;
        public int TotalStats => CharacterInfo.TotalStats;
        public int TotalRoles => CharacterInfo.TotalRoles;
        public int TotalSkills => CharacterInfo.TotalSkills;
        public int TotalDescriptors => CharacterInfo.TotalDescriptors;

        public PersonAttribute PersonAttribute { get; set; }
        public bool CanBuy => CanAdvance && Cost <= CurrentXp;
        public bool CanAdvance => PersonAttribute.AttributeType != AttributeType.Descriptor;

        public int Cost
        {
            get
            {
                switch (PersonAttribute.AttributeType)
                {
                    case AttributeType.Stat:
                        return TotalStats;
                    case AttributeType.Skill:
                        return PersonAttribute.CurrentValue + TotalAdvancesTaken + TotalSkills;
                    case AttributeType.Role:
                        return ((PersonAttribute.CurrentValue + TotalAdvancesTaken + 1)*4) + TotalRoles;
                    case AttributeType.Descriptor:
                        return -1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public CharacterLevelInfo CharacterInfo { get; set; }
        public string DisplayPrefix => PersonAttribute.CurrentValue >= 0 ? "+" : "-";
        public string DisplayValue  => $"{DisplayPrefix}{PersonAttribute.CurrentValue}";
    }
}