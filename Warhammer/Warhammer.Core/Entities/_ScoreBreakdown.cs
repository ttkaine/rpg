namespace Warhammer.Core.Entities
{
    public enum ScoreType
    {
        Total = 0,
        Image = 1,
        PageText = 2,
        Links = 3,
        Sessions = 4,
        Logs = 5,
        Awards = 6,
        Stats = 7,
        Roles = 8,
        Descriptors = 9,
        Level = 10,
        Places,
        OtherSessionLogs,
        People
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