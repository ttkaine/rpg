using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Abstract
{
    public interface IAdminSettingsProvider
    {
        string GetAdminSetting(AdminSettingName name);
        void SetAdminSettingValue(AdminSettingName name, string value);
        List<AdminSetting> AdminSettings();
    }
}