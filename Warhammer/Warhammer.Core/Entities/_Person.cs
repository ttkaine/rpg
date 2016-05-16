using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Entities
{
    public enum StatName
    {
        Combat = 1,
        Action = 2,
        Intellect = 3,
        Work = 4,
        Social = 5,
        Self = 6
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
                    CurrentXp = CurrentXp - RoleCost;
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
                    CurrentXp = CurrentXp - DescriptorCost;
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
                    CurrentXp = CurrentXp - StatCost;
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
                    temp.Add((StatName)personStat.StatId, personStat.CurrentValue);
                }

                foreach (int statId in Enum.GetValues(typeof(StatName)))
                {
                    StatName stat = (StatName)statId;
                    if (!temp.ContainsKey(stat))
                    {
                        temp.Add(stat, 0);
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
                return (int)(Math.Floor(DescriptorNames.Count * 2.5) - 4);
            }
        }

        public int StatCost
        {
          get { return Stats.Sum(s => s.Value) - 17; }
        }

        public IEnumerable<Session> Sessions
        {
            get { return Related.OfType<Session>(); }
        }

        public override double ActivityBonus
        {
            get
            {

                double score = ScoreBreakdown.Sum(s => s.ActivityBonus);
                return score;
            }
        }

        public IEnumerable<Award> OrderedAwards
        {
            get { return Awards.OrderBy(t => t.Trophy.TypeId == (int)TrophyType.DefaultAward).ThenBy(m => m.Trophy.TypeId).ThenByDescending(m => m.Trophy.PointsValue).ThenBy(a => a.Trophy.Name).ThenBy(a => a.Id ); }
        }

        public int ActivityScore
        {
            get
            {
                return (int)Math.Floor(ActivityBonus);
            }
        }

        public int PermenentScore
        { 
            get
            {
                double score = ScoreBreakdown.Sum(s => s.BaseValue);
                return (int)Math.Ceiling(Math.Round(score, 1));
            }
        }


        public List<ScoreBreakdown> ScoreBreakdown
        {
            get
            {
                List<ScoreHistory> scores = ScoreHistories.Where(s => s.DateTime == DateTime.Now.Date).ToList();
              
                List<ScoreBreakdown> breakdown = new List<ScoreBreakdown>();
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Sessions",
                    BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Sessions).Sum(s => s.PointsValue),
                    ActivityBonus = 0//sessions.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Session Logs",
                    BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Logs).Sum(s => s.PointsValue),
                    ActivityBonus = 0//logs.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Related Pages",
                    BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Links).Sum(s => s.PointsValue),
                    ActivityBonus = 0                  
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Page Text",
                    BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.PageText).Sum(s => s.PointsValue),
                    ActivityBonus = 0
                });
                if (scores.Any(s => s.ScoreType == ScoreType.Stats && s.PointsValue > 0))
                {
                    breakdown.Add(new ScoreBreakdown
                    {
                        Name = "Stats Value",
                        BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Stats).Sum(s => s.PointsValue),
                        ActivityBonus = 0
                    });
                    breakdown.Add(new ScoreBreakdown
                    {
                        Name = "Role Bonus",
                        BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Roles).Sum(s => s.PointsValue),
                        ActivityBonus = 0
                    });
                    breakdown.Add(new ScoreBreakdown
                    {
                        Name = "Descriptor Bonus",
                        BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Descriptors).Sum(s => s.PointsValue),
                        ActivityBonus = 0
                    });
                }
                double awardValue = (double)scores.Where(s => s.ScoreType == ScoreType.Awards).Sum(s => s.PointsValue);
                if (awardValue > 0)
                {
                    breakdown.Add(new ScoreBreakdown
                    {
                        Name = "Awards",
                        BaseValue = awardValue,
                        ActivityBonus = 0
                    });
                }
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Image Bonus",
                    BaseValue = (double)scores.Where(s => s.ScoreType == ScoreType.Image).Sum(s => s.PointsValue),
                    ActivityBonus = 0
                });
                return breakdown;
            }
        } 

        public override int PointsValue
        {
            get { return PermenentScore + ActivityScore; }
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


    }
}
