using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Entities
{
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
                    BaseValue = AddBonus ? sessions.Sum(l => l.BaseScore) * 2 : sessions.Sum(l => l.BaseScore),
                    ActivityBonus = sessions.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Session Logs",
                    BaseValue = AddBonus ? logs.Sum(l => l.BaseScore) * 2: logs.Sum(l => l.BaseScore),
                    ActivityBonus = logs.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Related Pages",
                    BaseValue = relatedPages.Sum(l => l.BaseScore),
                    ActivityBonus = 0                  
                });
                double awardValue = Awards.Where(a => a.Trophy.TypeId != (int) TrophyType.DefaultAward).Sum(a => a.Trophy.PointsValue);
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

        public bool AddBonus { get; set; }
    }
}
