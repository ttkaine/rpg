using System;
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
                if (Roles != null)
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
            if (CanBuyRole)
            {
                CurrentXp = CurrentXp - RoleCost;
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
            if (CanBuyDescriptor)
            {
                CurrentXp = CurrentXp - DescriptorCost;
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
                    stat.CurrentValue++;
                    stat.XpSpent = stat.XpSpent + StatCost;
                    CurrentXp = CurrentXp - StatCost;
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
                List<Page> relatedPages = Related.ToList();
                List<Session> sessions = Related.OfType<Session>().ToList();
				List<SessionLog> logs = (from session in sessions where SessionLogs.Count(l => l.SessionId == session.Id) > 0 select SessionLogs.First(l => l.SessionId == session.Id)).ToList();
                relatedPages = relatedPages.Where(s => !sessions.Contains(s)).ToList();
                relatedPages = relatedPages.Where(s => !logs.Contains(s)).ToList();

                List<ScoreBreakdown> breakdown = new List<ScoreBreakdown>();
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Sessions",
                    BaseValue = sessions.Sum(l => l.BaseScore),
                    ActivityBonus = sessions.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Session Logs",
                    BaseValue = InclueUplift ? logs.Sum(l => l.BaseScore) * UpliftFactor : logs.Sum(l => l.BaseScore),
                    ActivityBonus = logs.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Related Pages",
                    BaseValue = relatedPages.Sum(l => l.BaseScore),
                    ActivityBonus = 0                  
                });
                double awardValue = Awards.Sum(a => a.Trophy.PointsValue);
                if (PlayerId == null)
                {
                    awardValue = awardValue + Awards.Count;
                }
                if (awardValue != 0)
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
                    BaseValue = HasImage ? 1 : 0,
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
