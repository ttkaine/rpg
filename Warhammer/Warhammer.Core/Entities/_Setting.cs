namespace Warhammer.Core.Entities
{

    public enum SettingSection
    {
        EmailNotifications = 1,
        DailySummaryEmails = 2,
        ShadowMode = 3
    }

    public enum SettingNames
    {
        SendEmailOnMyTurn,
        SendEmailOnNewPage,
        SendEmailOnNewComment,
        SendEmailOnUpdatePage,
        SendDailySummaryEmail,
        ShadowMode
    }

    public partial class Setting
    {
        public SettingSection SettingSection
        {
            get { return (SettingSection)SectionId; }
            set { SectionId = (int)value; }
        }
    }
}
