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
        SendingSmtpAccount = 105,
        SendGridKey = 106,

        //Configuration Options 200
        UseHangfire = 201,
        AzureStorageConnectionString = 202
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