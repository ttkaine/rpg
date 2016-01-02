using System.Linq;

namespace Warhammer.Core.Entities
{
    public partial class Player
    {
        public bool SettingIsEnabled(Setting setting)
        {
            return UserSettings.Any(s => s.Enabled && s.SettingId == (int) setting);
        }

    }
}
