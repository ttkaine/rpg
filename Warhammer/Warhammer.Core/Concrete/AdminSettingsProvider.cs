using System;
using System.Collections.Generic;
using System.Linq;
using LazyCache;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class AdminSettingsProvider : IAdminSettingsProvider
    {
        readonly IRepository _repository;
        readonly IAuthenticatedUserProvider _authenticatedUser;
        private readonly IAppCache _cache;

        private string CacheKey => $"admin-Settings-for-campaign-{_repository.CurrentCampaignId}";
        public AdminSettingsProvider(IRepository repo, IAuthenticatedUserProvider authenticatedUser, IAppCache cache)
        {
            _repository = repo;
            _authenticatedUser = authenticatedUser;
            _cache = cache;
        }

        public bool CurrentUserIsAdmin
        {
            get { return _authenticatedUser.IsAdmin; }
        }

        public string GetAdminSetting(AdminSettingName name)
        {
            AdminSetting setting = AdminSettings().FirstOrDefault(s => s.SettingId == (int)name);
            return setting?.SettingValue;
        }

        public void SetAdminSettingValue(AdminSettingName name, string value)
        {
            if (!CurrentUserIsAdmin)
            {
                throw new Exception("Not an Admin");
            }

            AdminSetting setting = _repository.AdminSettings().FirstOrDefault(s => s.SettingId == (int)name);
            if (setting == null)
            {
                setting = new AdminSetting
                {
                    SettingId = (int)name
                };
            }

            setting.SettingValue = value;

            _repository.Save(setting);
            _cache.Remove(CacheKey);
        }

        public List<AdminSetting> AdminSettings()
        {
            return _cache.GetOrAdd(CacheKey, GetAdminSettings);
        }

        private List<AdminSetting> GetAdminSettings()
        {
            var vals = Enum.GetValues(typeof(AdminSettingName));

            List<int> nameValues = vals.Cast<int>().OrderByDescending(i => i).ToList();

            List<AdminSetting> settings = _repository.AdminSettings().ToList();

            foreach (int nameValue in nameValues)
            {
                if (settings.All(s => s.SettingId != nameValue))
                {
                    settings.Add(new AdminSetting
                    {
                        SettingId = nameValue,
                        SettingValue = ""
                    });
                }
            }

            return settings;
        }
    }
}