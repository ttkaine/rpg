namespace Warhammer.Core.Entities
{

    public enum AdminSettingName
    {
        //Core Site Settings 0
        SiteName = 1,
        //CssOverrideFilename = 2,


        //email setting 100
        SendingEmailAddress = 101,
        SendingEmailName = 102,
        SendingEmailPassword = 103,
        SendingSmtpServer = 104,
    }

    public partial class AdminSetting
    {
        public AdminSettingName Name
        {
            get { return (AdminSettingName)SettingId; }
            set { SettingId = (int)value; }
        }
    }
}