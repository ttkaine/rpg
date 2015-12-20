using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Warhammer.Tests.Smoke
{
    public enum TestSettingType
    {
        Dev,
        Ci,
        Test,
        Live,
        Pirate
    }

    public class TestSettings
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        public string UpdateUrl
        {
            get { return _settings["UpdateUrl"]; }
        }

        public string BaseUrl
        {
            get { return _settings["BaseUrl"]; }
        }

        public string DefaultUsername
        {
            get { return _settings["DefaultUsername"]; }
        }

        public string DefaultPassword
        {
            get { return _settings["DefaultPassword"]; }
        }

        public string StandardUsername
        {
            get { return _settings["StandardUsername"]; }
        }

        public string StandardPassword
        {
            get { return _settings["StandardPassword"]; }
        }

        public TestSettingType SettingsType { get; set; }

        public bool LoadSettings(TestSettingType settings)
        {
            try
            {
                SettingsType = settings;
                string settingsLocation = "";
                switch (settings)
                {
                    case TestSettingType.Dev:
                        settingsLocation = ConfigurationManager.AppSettings.Get("DevSettingsLocation");
                        break;
                    case TestSettingType.Ci:
                        settingsLocation = ConfigurationManager.AppSettings.Get("CiSettingsLocation");
                        break;
                    case TestSettingType.Test:
                        settingsLocation = ConfigurationManager.AppSettings.Get("TestSettingsLocation");
                        break;
                    case TestSettingType.Live:
                        settingsLocation = ConfigurationManager.AppSettings.Get("LiveSettingsLocation");
                        break;
                    case TestSettingType.Pirate:
                        settingsLocation = ConfigurationManager.AppSettings.Get("PirateSettingsLocation");
                        break;
                }

                string fileContent = File.ReadAllText(settingsLocation);
                string[] lines = fileContent.Split(Environment.NewLine.ToCharArray());
                foreach (var line in lines)
                {
                    string[] dataItems = line.Split('|');
                    if (dataItems.Length == 2)
                    {
                        _settings.Add(dataItems[0], dataItems[1]);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
