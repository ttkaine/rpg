using System;

namespace Warhammer.Core.Entities
{

    public enum Setting
    {
        SendEmailOnMyTurn = 1,
        SendEmailOnNewPage = 2,
        SendEmailOnNewComment = 3,
        SendEmailOnUpdatePage = 4,
        SendDailySummaryEmail = 5
    }

    public partial class UserSetting
    {
        public Setting Setting
        {
            get { return (Setting) SettingId; }
            set { SettingId = (int)value; }
        }
    }
}
