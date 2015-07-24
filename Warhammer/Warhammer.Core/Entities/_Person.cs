using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Entities
{
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
                double bonus = base.ActivityBonus * 5;
                bonus = bonus + Sessions.Sum(s => s.ActivityBonus);
                bonus = bonus + SessionLogs.Sum(s => s.ActivityBonus);
                return bonus;
            }
        }

        private double PersonScore
        {
            get
            {
                double score = base.BaseScore;
                List<SessionLog> logs = SessionLogs.ToList();
                List<Page> relatedPages = Related.ToList();
                
                relatedPages.AddRange(Related1.ToList());
                score = score + relatedPages.Where(s => !logs.Contains(s)).Sum(l => l.BaseScore);
                score = score + logs.Sum(l => l.BaseScore);
                score = score + Awards.Sum(a => a.Trophy.PointsValue);
                if (HasImage)
                {
                    score = score + 10;
                }
                return score;
            }
        }

        public override int PointsValue
        {
            get { return (int) PersonScore + (int) BaseScore + (int) ActivityBonus; }
        }
    }
}
