using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Entities
{
    public enum StatName
    {
        //0 CROW
        Combat = 1,
        Action = 2,
        Intellect = 3,
        Work = 4,
        Social = 5,
        Self = 6,

        //100  FATE / FUHammer
        Weapon_Skill = 101,
        Ballistics_Skill = 102,
        Strength = 103,
        Toughness = 104,
        Agility = 105,
        Intelligence = 106,
        Willpower = 107,
        Fellowship = 108
    }

    public struct ScoreBreakdown
    {
        public string Name { get; set; }
        public double BaseValue { get; set; }
        public double ActivityBonus { get; set; }
        public double Score { get { return BaseValue + ActivityBonus; } }
    }



    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class Person
    {
        public bool IsFuCharacter
        {
            get { return PersonStats.Any(p => p.StatId > 100 && p.StatId < 200); }
        }

        public bool IsCrowCharacter
        {
            get { return PersonStats.Any(p => p.StatId > 0 && p.StatId < 100); }
        }

        public int BaseStatXpModifier
        {
            get
            {
                if (IsCrowCharacter)
                {
                    return -17;
                }

                if (IsFuCharacter)
                {
                    return -5;
                }


                return 0;
            }
        }

        public bool IsFavourite
        {
            get
            {
                return
                    Awards.Any(
                        a =>
                            a.Trophy.TypeId == (int) TrophyType.FirstFavouriteNpc ||
                            a.Trophy.TypeId == (int) TrophyType.SecondFavouriteNpc ||
                            a.Trophy.TypeId == (int) TrophyType.ThirdFavouriteNpc);
            }
        }

        public int StatValue(StatName name)
        {
            if (Stats.Any(s => s.Key == name))
            {
                return Stats.FirstOrDefault(s => s.Key == name).Value;
            }
            return -1;
        }

        private const char Seperator = '¬';
        public List<string> RoleNames
        {
            get
            {
                List<string> temp = new List<string>();
                if (Roles != null)
                {
                    string[] roles = Roles.Split(Seperator);

                    foreach (string role in roles)
                    {
                        if (!string.IsNullOrWhiteSpace(role) && !temp.Contains(role))
                        {
                            temp.Add(role);
                        }
                    }
                }
                return temp;
            }
        }

        public List<string> DescriptorNames
        {
            get
            {
                List<string> temp = new List<string>();
                if (Descriptors != null)
                {
                    string[] descriptors = Descriptors.Split(Seperator);

                    foreach (string descriptor in descriptors)
                    {
                        if (!string.IsNullOrWhiteSpace(descriptor) && !temp.Contains(descriptor))
                        {
                            temp.Add(descriptor);
                        }
                    }
                }
                return temp;
            }
        }

        public bool AddRole(string roleName, bool free = false)
        {
            if (CanBuyRole || free)
            {
                if (!free)
                {
                    XpSpent = XpSpent + RoleCost;
                }
                if (string.IsNullOrWhiteSpace(Roles))
                {
                    Roles = roleName;
                }
                else
                {
                    Roles = Roles + Seperator + roleName;
                }
                return true;
            }
            return false;
        }

        public bool AddDescriptor(string descriptor, bool free = false)
        {
            if (CanBuyDescriptor || free)
            {
                if (!free)
                {
                    XpSpent = XpSpent + DescriptorCost;
                }
                if (string.IsNullOrWhiteSpace(descriptor))
                {
                    Descriptors = descriptor;
                }
                else
                {
                    Descriptors = Descriptors + Seperator + descriptor;
                }
                return true;
            }
            return false;
        }

        public bool BuyStatIncrease(StatName statName)
        {
            if (CanBuyStat)
            {
                PersonStat stat = PersonStats.FirstOrDefault(s => s.StatId == (int) statName);
                if (stat != null)
                {
                    XpSpent = XpSpent + StatCost;
                    stat.XpSpent = stat.XpSpent + StatCost;
                    stat.CurrentValue++;
                    return true;
                }
            }
            return false;
        }

        public Dictionary<StatName, int> Stats
        {
            get
            {
                Dictionary < StatName, int> temp = new Dictionary<StatName, int>();
                foreach (PersonStat personStat in PersonStats)
                {
                    if (!temp.ContainsKey((StatName) personStat.StatId))
                    {
                        temp.Add((StatName) personStat.StatId, personStat.CurrentValue);
                    }
                }
                return temp;
            }
        }

        public bool CanBuyStat
        {
            get { return CurrentXp >= StatCost; }
        }
        public bool CanBuyRole
        {
            get { return CurrentXp >= RoleCost; }
        }
        public bool CanBuyDescriptor
        {
            get { return CurrentXp >= DescriptorCost; }
        }
        public int RoleCost
        {
            get
            {
                return RoleNames.Count * 3 + 2;
            }
        }

        public int DescriptorCost
        {
            get
            {
                int cost = 0;
                if (IsNpc)
                {
                    cost = (DescriptorNames.Count * 2) + 1;
                }
                else
                {
                    cost = (DescriptorNames.Count * 2) - 4;
                }

                if (cost < 1)
                {
                    cost = 1;
                }

                if (IsFuCharacter)
                {
                    cost++;
                }

                return cost;
            }
        }

        public bool IsNpc => !PlayerId.HasValue;

        public int StatCost
        {
            get
            {
                if (!Stats.Any())
                {
                    return 100;
                }
             
                int cost = Stats.Sum(s => s.Value) + BaseStatXpModifier;
                if (cost < 1)
                {
                    cost = 1;
                }

                if (IsFuCharacter)
                {
                    cost++;
                }

                return cost;
            }
        }

        public IEnumerable<Session> Sessions
        {
            get { return Related.OfType<Session>(); }
        }

        public IEnumerable<Award> OrderedAwards
        {
            get { return Awards.OrderBy(t => t.Trophy.TypeId == (int)TrophyType.DefaultAward).ThenBy(m => m.Trophy.TypeId).ThenByDescending(m => m.Trophy.PointsValue).ThenBy(a => a.Trophy.Name).ThenBy(a => a.Id ); }
        }

        public int PointsValue
        {
            get { return (int)Math.Ceiling(CurrentScore); }
        }

        public string MiniSummary
        {
            get
            {
                return GetSummary(250);
            }
        }

        public string SearchString
        {
            get
            {
                string[] awards = Awards.Select(a => a.Trophy.Name).ToArray();
                string[] reasons = Awards.Select(a => a.Reason).ToArray();
                return string.Format("{0} - {1} - {2}", RawText,
                    string.Join(",",awards ),
                    string.Join(",", reasons));
            }
        }

        public bool InclueUplift { get; set; }
        public double UpliftFactor { get; set; }

        public decimal CurrentXp => XPAwarded - XpSpent;


        public bool CanBuyHitSlot(SimpleHitPointLevel level, SimpleHitPointType type)
        {
            if (SimpleHitPoints.Any(s => s.Purchased.HasValue && s.HitPointLevelId == (int) level && s.HitPointTypeId == (int) type))
            {
                return false;
            }

            return CurrentXp >= HitSlotCost(level, type);
        }

        public int HitSlotCost(SimpleHitPointLevel level, SimpleHitPointType type)
        {
            return (int)level + SimpleHitPoints.Count(s => s.Purchased.HasValue && s.HitPointTypeId == (int)type);
        }

        public int TotalPennies
        {
            get
            {
                int total = 0;
                if (Pennies.HasValue)
                {
                    total = total + Pennies.Value;
                }
                if (Shillings.HasValue)
                {
                    total = total + Shillings.Value*12;
                }
                if (Crowns.HasValue)
                {
                    total = total + Crowns.Value*240;
                }
                return total;
            }
        }
    }
}
