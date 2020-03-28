using System;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Models
{
    public class PersonAttributeAdvanceModel
    {
        public static int StartingWear = 2;
        public static int StartingHarm = 6;
        public static int StartingStats = 18;

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
        public bool CanAdvance => PersonAttribute.AttributeType != AttributeType.Descriptor && PersonAttribute.AttributeType != AttributeType.Edge;

        public string SaleName
        {
            get
            {
                switch (PersonAttribute.AttributeType)
                {
                    case AttributeType.Stat:
                    case AttributeType.Skill:
                    case AttributeType.Role:
                    case AttributeType.Discipline:
                    case AttributeType.Resilience:
                    case AttributeType.Resolve:
                    case AttributeType.Magic:
                    case AttributeType.MagicItem:
                        return $"One point of {Name} for {SaleValue} XP";
                    case AttributeType.Descriptor:
                        return $"{Name} for {SaleValue} XP";
                    case AttributeType.Wear:
                        return $"One point from Wear ({Value}) for {SaleValue} XP";
                    case AttributeType.Harm:
                        return $"One point from Harm ({Value}) for {SaleValue} XP";
                    case AttributeType.Edge:
                        return $"One Edge for {SaleValue} XP";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        } //=> $"{Name} for {SaleValue} XP";

        public int SaleValue
        {
            get
            {
                switch (PersonAttribute.AttributeType)
                {
                    case AttributeType.Stat:
                    case AttributeType.Magic:
                    case AttributeType.MagicItem:
                        if (TotalStats <= StartingStats)
                            return -1;

                        int totalValue = TotalStats;
                        totalValue = totalValue - StartingStats;
                        if (totalValue < 1)
                        {
                            totalValue = 1;
                        }
                        totalValue = totalValue * totalValue;
                        if (totalValue <= CharacterInfo.XpSpent)
                        {
                            return totalValue;
                        }
                        return -1;
                    case AttributeType.Resolve:
                        int resolveAdvance = PersonAttribute.CurrentValue;
                        if (resolveAdvance < 2)
                        {
                            return -1;
                        }

                        if (resolveAdvance <= CharacterInfo.XpSpent)
                        {
                            return resolveAdvance;
                        }
                        return -1;
                    case AttributeType.Skill:
                        int skillAdvance = PersonAttribute.CurrentValue;
                        if (skillAdvance < 1)
                        {
                            skillAdvance = 1;
                        }

                        if (skillAdvance <= CharacterInfo.XpSpent)
                        {
                            return skillAdvance;
                        }
                        return -1;

                    case AttributeType.Resilience:
                        int resilienceAdvance = PersonAttribute.CurrentValue;
                        if (resilienceAdvance < 2)
                        {
                            return -1;
                        }

                        resilienceAdvance = resilienceAdvance * 4;
                        if (resilienceAdvance <= CharacterInfo.XpSpent)
                        {
                            return resilienceAdvance;
                        }
                        return -1;

                    case AttributeType.Role:
                    case AttributeType.Discipline:

                        int roleAdvance = PersonAttribute.CurrentValue;
                        if (roleAdvance < 1)
                        {
                            roleAdvance = 1;
                        }

                        roleAdvance = roleAdvance * 4;
                        if (roleAdvance <= CharacterInfo.XpSpent)
                        {
                            return roleAdvance;
                        }
                        return -1;
                    case AttributeType.Descriptor:
                        int descriptor = TotalDescriptors;
                        if(descriptor > 2)
                        {
                            descriptor--;
                            if (descriptor <= CharacterInfo.XpSpent)
                            {
                                return descriptor;
                            }
                        }

                        return -1;
                    case AttributeType.Edge:
                        if (CharacterInfo.TotalEdge > 1)
                        {
                            int edgeLevel = CharacterInfo.TotalEdge;
                            edgeLevel = edgeLevel * edgeLevel * edgeLevel;
                            if (edgeLevel <= CharacterInfo.XpSpent)
                            {
                                return edgeLevel;
                            }
                        }
                        return -1;
                    case AttributeType.Wear:
                        if (CharacterInfo.TotalWear > StartingWear)
                        {
                            int wearValue = CharacterInfo.TotalWear - StartingWear;
                            if (wearValue < 1)
                            {
                                wearValue = 1;
                            }
                            if (wearValue <= CharacterInfo.XpSpent)
                            {
                                return wearValue;
                            }
                        }
                        return -1;

                    case AttributeType.Harm:
                        if (CharacterInfo.TotalHarm > StartingHarm)
                        {
                            int harmValue = CharacterInfo.TotalHarm - StartingHarm;
                            if (harmValue < 1)
                            {
                                harmValue = 1;
                            }

                            if (harmValue <= CharacterInfo.XpSpent)
                            {
                                return harmValue;
                            }
                        }
                        return -1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public int Cost
        {
            get
            {
                switch (PersonAttribute.AttributeType)
                {
                    case AttributeType.Stat:
                    case AttributeType.Magic:
                    case AttributeType.MagicItem:
                        int totalValue = TotalStats;
                        totalValue = totalValue - StartingStats;
                        totalValue++;
                        if (totalValue < 1)
                        {
                            totalValue = 1;
                        }
                        totalValue = totalValue * totalValue;
                        return totalValue;
                    case AttributeType.Resolve:
                    case AttributeType.Skill:
                        int skillAdvance = PersonAttribute.CurrentValue;
                        skillAdvance++;
                        if (skillAdvance < 1)
                        {
                            skillAdvance = 1;
                        }
                        return skillAdvance;
                    case AttributeType.Role:
                    case AttributeType.Discipline:
                    case AttributeType.Resilience:
                        int roleAdvance = PersonAttribute.CurrentValue;
                        roleAdvance++;
                        if (roleAdvance < 1)
                        {
                            roleAdvance = 1;
                        }
                        return roleAdvance * 4;
                    case AttributeType.Descriptor:
                    case AttributeType.Edge:
                        return -1;
                    case AttributeType.Wear:
                        int wearValue = CharacterInfo.TotalWear - StartingWear;
                        wearValue++;
                        if (wearValue < 1)
                        {
                            wearValue = 1;
                        }
                        return wearValue;
                    case AttributeType.Harm:
                        int harmValue = CharacterInfo.TotalHarm - StartingHarm;
                        harmValue++;
                        if (harmValue < 1)
                        {
                            harmValue = 1;
                        }
                        return harmValue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public CharacterLevelInfo CharacterInfo { get; set; }
        public string DisplayPrefix => PersonAttribute.CurrentValue >= 0 ? "+" : "";
        public string DisplayValue {
            get
            {
                if (CharacterInfo.FixedWearAndHarm)
                {
                    if (PersonAttribute.AttributeType == AttributeType.Harm ||
                        PersonAttribute.AttributeType == AttributeType.Wear)
                    {

                        FixedWearAndHarmLevels level = (FixedWearAndHarmLevels) PersonAttribute.CurrentValue;
                        return $"{DisplayPrefix}{PersonAttribute.CurrentValue} {level}";
                    }
                }

                return $"{DisplayPrefix}{PersonAttribute.CurrentValue}";
            }
    } 

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

        public int Value => PersonAttribute.CurrentValue;
        public bool CanSell => SaleValue > 0;
    }
}