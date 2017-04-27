using System;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class PersonAttributeAdvanceModel
    {
        public int Id => PersonAttribute.Id;
        public string Name => PersonAttribute.Name;

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
                        int totalValue = TotalAdvancesTaken + TotalStats;
                        totalValue = totalValue - CharacterInfo.TotalAverageStatValue;
                        if (totalValue < 1)
                        {
                            totalValue = 1;
                        }
                        return totalValue;
                    case AttributeType.Skill:
                        int skillAdvance = PersonAttribute.CurrentValue;
                        skillAdvance = skillAdvance/2;

                        skillAdvance = skillAdvance + TotalAdvancesTaken;

                        if (skillAdvance < 1)
                        {
                            skillAdvance = 1;
                        }
                        return skillAdvance;
                    case AttributeType.Role:
                        int roleAdvance = PersonAttribute.CurrentValue*2;
                        roleAdvance = roleAdvance + TotalAdvancesTaken;
                        if (roleAdvance < 1)
                        {
                            roleAdvance = 1;
                        }
                        return roleAdvance;
                    case AttributeType.Descriptor:
                        return -1;
                    case AttributeType.Wear:
                    case AttributeType.Harm:
                        return (PersonAttribute.CurrentValue + TotalAdvancesTaken)/2;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public CharacterLevelInfo CharacterInfo { get; set; }
        public string DisplayPrefix => PersonAttribute.CurrentValue >= 0 ? "+" : "-";
        public string DisplayValue => $"{DisplayPrefix}{PersonAttribute.CurrentValue}";

        public bool Exhausted
        {
            get
            {
                if (PersonAttribute.AttributeType == AttributeType.Wear || PersonAttribute.AttributeType == AttributeType.Harm)
                {
                    return !string.IsNullOrWhiteSpace(PersonAttribute.Name);
                }
                return false;
            }
        }
    }
}