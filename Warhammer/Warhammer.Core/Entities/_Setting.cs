namespace Warhammer.Core.Entities
{

    public enum SettingSection
    {
        EmailNotifications = 1
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
