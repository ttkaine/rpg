using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

        public List<ScoreBreakdown> ScoreBreakdown
        {
            get
            {
                List<SessionLog> logs = SessionLogs.ToList();
                List<Page> relatedPages = Related.ToList();
                List<Session> sessions = Related.OfType<Session>().ToList();
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
                    BaseValue = logs.Sum(l => l.BaseScore),
                    ActivityBonus = SessionLogs.Sum(s => s.ActivityBonus)
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Related Pages",
                    BaseValue = relatedPages.Sum(l => l.BaseScore),
                    ActivityBonus = 0                  
                });
                breakdown.Add(new ScoreBreakdown
                {
                    Name = "Awards",
                    BaseValue = Awards.Sum(a => a.Trophy.PointsValue),
                    ActivityBonus = 0
                });
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
            get
            {
                double score = ScoreBreakdown.Sum(s => s.Score);
                return (int)Math.Ceiling(score);
            }
        }

        public string MiniSummary
        {
            get
            {
                const int summaryLength = 100;
                string text = Regex.Replace(Description, "<.*?>", string.Empty);
                const string str = "...";
                if (!string.IsNullOrWhiteSpace(text) && text.Length > summaryLength)
                {
                    text = text.Substring(0, summaryLength - str.Length) + str;
                }
                return text;
            }
        }
    }
}
