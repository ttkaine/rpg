using System.Collections.Generic;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class UserSettingsViewModel
    {
        public UserSettingsViewModel()
        {
            SectionsIds = new List<SettingSection>();
        }

        public string User { get; set; }
        public List<SettingSection> SectionsIds { get; set; } 
    }

    public class UserSettingsSectionViewModel
    {
        public UserSettingsSectionViewModel()
        {
            Settings = new List<UserSettingViewModel>();
        }

        public string SettingTitle { get; set; }
        public List<UserSettingViewModel> Settings { get; set; }
    }

    public class UserSettingViewModel
    {
        public int SettingId { get; set; }
        public string EnabledText { get; set; }
        public string DisabledText { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
    }
}