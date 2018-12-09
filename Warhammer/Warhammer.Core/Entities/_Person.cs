using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;

namespace Warhammer.Core.Entities
{
    public enum Gender
    {
        Unknown = 0,
        Female = 1,
        Male = 2,
        Hermaphrodite = 3,
        Neuter = 4,
        Other = 5
    }

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
        Ballistic_Skill = 102,
        Strength = 103,
        Toughness = 104,
        Agility = 105,
        Intelligence = 106,
        Willpower = 107,
        Fellowship = 108
    }

    public enum HeroLevel
    {
        Rookie = 0,
        Adventurer,
        Veteran,
        Hero,
        Champion,
        Legend
    }

    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class Person
    {
        public Gender Gender
        {
            get { return (Gender) (GenderEnum ?? 0); }
            set { GenderEnum = (int) value;  }
        }

        public int XpLevel => (int) Math.Floor(TotalAdvancesTaken / 5.0);

        public static HeroLevel GetHeroLevel(decimal xp)
        {
            if (xp > 1200) return HeroLevel.Legend;
            if (xp > 600) return HeroLevel.Champion;
            if (xp > 200) return HeroLevel.Hero;
            if (xp > 50) return HeroLevel.Veteran;
            if (xp > 10) return HeroLevel.Adventurer;
            return HeroLevel.Rookie;
        }

        public HeroLevel HeroLevel => GetHeroLevel(XPAwarded);

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

        //public IEnumerable<Award> OrderedAwards
        //{
        //    get { return Awards.OrderBy(t => t.Trophy.TypeId == (int)TrophyType.DefaultAward).ThenBy(m => m.Trophy.TypeId).ThenByDescending(m => m.Trophy.PointsValue).ThenBy(a => a.Trophy.Name).ThenBy(a => a.Id ); }
        //}

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

        //public string SearchString
        //{
        //    get
        //    {
        //        string[] awards = Awards.Select(a => a.Trophy.Name).ToArray();
        //        string[] reasons = Awards.Select(a => a.Reason).ToArray();
        //        return string.Format("{0} - {1} - {2}", RawText,
        //            string.Join(",",awards ),
        //            string.Join(",", reasons));
        //    }
        //}

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

        [Display(Name = "NPC or Player Character?")]
        public bool CreateAsNpc { get; set; }

        public bool AttributeMoveTaken { get; set; }

        public void DeductMoney(int penniesDeducted)
        {
            if (TotalPennies > 0 && Pennies.HasValue && Shillings.HasValue && Crowns.HasValue)
            {
                if (Pennies < penniesDeducted)
                {
                    if (penniesDeducted >= 240)
                    {
                        int crownsDeducted = penniesDeducted / 240;

                        Crowns = Crowns - crownsDeducted;
                        penniesDeducted = penniesDeducted - crownsDeducted * 240;
                    }

                    if (penniesDeducted >= 12)
                    {
                        int shillingsDeducted = penniesDeducted/12;
                        Shillings = Shillings - shillingsDeducted;

                        penniesDeducted = penniesDeducted - (shillingsDeducted * 12);
                    }
                }

                Pennies = Pennies.Value - penniesDeducted;

                //adjust coins to minimize -ve values

                if (Crowns < 0 && Shillings > 20)
                {
                    int availableCrowns = Shillings.Value/20;
                    int neededCrowns = Crowns.Value;

                    if (neededCrowns < availableCrowns)
                    {
                        Crowns = 0;
                        Shillings = Shillings.Value - (neededCrowns*20);
                    }
                    else
                    {
                        Crowns = Crowns + availableCrowns;
                        Shillings = Shillings.Value - (availableCrowns*20);
                    }
                }


                if (Shillings < 0 && Pennies > 20)
                {
                    int availableShillings = Pennies.Value / 12;
                    int neededShillings = Shillings.Value;

                    if (neededShillings < availableShillings)
                    {
                        Shillings = 0;
                        Pennies = Pennies.Value - (neededShillings * 12);
                    }
                    else
                    {
                        Shillings = Shillings + availableShillings;
                        Pennies = Pennies.Value - (availableShillings * 12);
                    }
                }



                if (Pennies < 0 && TotalPennies > 0)
                {
                    int penniesNeeded = 0 - Pennies.Value;
                    int shillingsToBreak = 20 * (int)Math.Round(penniesNeeded / 12.0);
                    Pennies = Pennies.Value + (shillingsToBreak * 12);
                    Shillings = Shillings.Value - shillingsToBreak;
                }

                if (Shillings < 0 && Crowns > 0)
                {
                    int shillingsNeeded = 0 - Shillings.Value;
                    if (Crowns*20 > shillingsNeeded)
                    {
                        int crownsToBreak = ((20*(int) Math.Floor(shillingsNeeded/20.0)) / 20) + 1;
                        Shillings = Shillings.Value + (crownsToBreak*20);
                        Crowns = Crowns.Value - crownsToBreak;
                    }
                    else
                    {
                        Shillings = Shillings.Value + (Crowns.Value*20);
                        Crowns = 0;
                    }
                }

                if (Pennies < 0 && Shillings > 0)
                {
                    int penniesNeeded = 0 - Pennies.Value;
                    if (Shillings*12 > penniesNeeded)
                    {
                        int shillingsToBreak = ((12*(int) Math.Floor(penniesNeeded/12.0))/12) + 1;
                        Pennies = Pennies.Value + (shillingsToBreak*12);
                        Shillings = Shillings.Value - shillingsToBreak;
                    }
                    else
                    {
                        Pennies = Pennies.Value + (Shillings.Value*20);
                        Shillings = 0;
                    }
                }

                if (Pennies < 0 && Crowns > 0)
                {
                    int penniesNeeded = 0 - Pennies.Value;
                    if (Crowns * 240 > penniesNeeded)
                    {
                        int crownsToBreak = ((240 * (int)Math.Floor(penniesNeeded / 240.0)) / 240) + 1;
                        Pennies = Pennies.Value + (crownsToBreak * 240);
                        Crowns = Crowns.Value - crownsToBreak;
                    }
                    else
                    {
                        Pennies = Pennies.Value + (Crowns.Value * 240);
                        Crowns = 0;
                    }

                    if (Pennies > 12)
                    {
                        int shillingsToAdd = Pennies.Value/12;
                        Pennies = Pennies.Value - (shillingsToAdd*12);
                        Shillings = Shillings.Value + shillingsToAdd;
                    }
                }

            }
        }

        public void AddMoney(int upkeep)
        {
            int crowns = upkeep/240;
            Crowns = Crowns.Value + crowns;
            upkeep = upkeep - (crowns*240);

            int shillings = upkeep/12;
            Shillings = Shillings.Value + shillings;
            upkeep = upkeep - (shillings*12);

            Pennies = Pennies.Value + upkeep;
        }
    }
}
