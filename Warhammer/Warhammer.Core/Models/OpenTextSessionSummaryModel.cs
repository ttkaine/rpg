using System;

namespace Warhammer.Core.Models
{
    public class OpenTextSessionSummaryModel
    {
        public string CssClass
        {
            get
            {
                if (MyTurn)
                {
                    return "btn-success";
                }
                return "btn-default";
            }
        }

        public DateTime LastPostTime { get; set; }

        public int DaysSinceLastPost => (int)Math.Floor((DateTime.Today - LastPostTime.Date).TotalDays);
        public string Domain { get; set; }
        public string RoleplayUrl => $"https://{Domain}/Roleplay/index/{SessionId}";
        public string PageUrl => $"https://{Domain}/Page/index/{SessionId}";
        public int SessionId { get; set; }
        public string SessionName { get; set; }

        public bool IsUpdated { get; set; }

        public bool IsPrivate { get; set; }
        public bool MyTurn { get; set; }
        public int CampaginId { get; set; }
    }
}