using System.ComponentModel.DataAnnotations;

namespace Warhammer.Core.Entities
{
    public enum ScoreType
    {
        Total = 0,
        Image = 1,
        [Display(Name = "Page Text")]
        PageText = 2,
        Links = 3,
        People = 4,
        Sessions = 5,
        Logs = 6,
        [Display(Name = "Other Logs")]
        OtherSessionLogs = 7,
        Awards = 8,
        Stats = 9,
        Roles = 10,
        Skills = 11,
        Descriptors = 12,
        Level = 13,
        Places = 14,
        Wear = 15,
        Magic = 16,
        Discipline
    }


    public partial class ScoreBreakdown
    {
        public ScoreType ScoreType
        {
            get { return (ScoreType)ScoreTypeId; }
            set { ScoreTypeId = (int)value; }
        }
    }
}