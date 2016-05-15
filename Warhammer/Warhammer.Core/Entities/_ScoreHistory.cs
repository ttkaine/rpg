namespace Warhammer.Core.Entities
{
    public enum ScoreType
    {
        Total = 0,
        Sessions = 1,
        Logs = 2,
        Links = 3,
        Awards = 4,
        Image = 5,
        PageText = 6,
        Stats = 7,
        Roles,
        Descriptors
    }

    public partial class ScoreHistory
    {
        public ScoreType ScoreType
        {
            get { return (ScoreType)ScoreTypeId; }
            set { ScoreTypeId = (int)value; }
        }
    }
}
