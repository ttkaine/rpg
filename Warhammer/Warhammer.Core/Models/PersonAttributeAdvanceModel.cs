using System;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class PersonAttributeAdvanceModel
    {
        public int Id => PersonAttribute.Id;
        public string Name => PersonAttribute.Name;
        public int CharacterLevel => CharacterInfo.CharacterLevel;
        public decimal CurrentXp => CharacterInfo.CurrentXp;
        public int TotalAdvancesTaken => CharacterInfo.TotalAdvancesTaken;
        public int TotalStats => CharacterInfo.TotalStats;
        public int TotalRoles => CharacterInfo.TotalRoles;
        public int TotalSkills => CharacterInfo.TotalSkills;
        public int TotalDescriptors => CharacterInfo.TotalDescriptors;

        public PersonAttribute PersonAttribute { get; set; }
        public bool CanBuy => CanAdvance && Cost <= CurrentXp;
        public bool CanAdvance => PersonAttribute.AttributeType != AttributeType.Descriptor && PersonAttribute.AttributeType != AttributeType.Edge;

        public int Cost
        {
            get
            {
                switch (PersonAttribute.AttributeType)
                {
                    case AttributeType.Stat:
                        int totalValue = TotalStats;
                        totalValue = totalValue - CharacterInfo.TotalAverageStatValue;
                        if (totalValue < 1)
                        {
                            totalValue = 1;
                        }
                        totalValue = totalValue* totalValue;

                        return totalValue + CharacterLevel;
                    case AttributeType.Skill:
                        int skillAdvance = PersonAttribute.CurrentValue * PersonAttribute.CurrentValue;

                        skillAdvance = skillAdvance / 4;

                        if (skillAdvance < 1)
                        {
                            skillAdvance = 1;
                        }
                        return skillAdvance + CharacterLevel;
                    case AttributeType.Role:
                        int roleAdvance = PersonAttribute.CurrentValue * PersonAttribute.CurrentValue;
                        return roleAdvance + CharacterLevel;
                    case AttributeType.Descriptor:
                    case AttributeType.Edge:
                        return -1;
                    case AttributeType.Wear:
                        int wearValue = CharacterInfo.TotalWear / 4;
                        if (wearValue < 1)
                        {
                            wearValue = 1;
                        }
                        return wearValue + CharacterLevel;
                    case AttributeType.Harm:
                        int harmValue = CharacterInfo.TotalHarm / 4;
                        if (harmValue < 1)
                        {
                            harmValue = 1;
                        }
                        return harmValue + CharacterLevel;
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
                if (PersonAttribute.AttributeType == AttributeType.Wear || PersonAttribute.AttributeType == AttributeType.Harm || PersonAttribute.AttributeType == AttributeType.Edge)
                {
                    return !string.IsNullOrWhiteSpace(PersonAttribute.Name);
                }
                return false;
            }
        }
    }
}