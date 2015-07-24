using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Warhammer.Tests.Integration
{
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

        public string Username
        {
            get { return _settings["Username"]; }
        }

        public string Password
        {
            get { return _settings["Password"]; }
        }

        public bool LoadSettings()
        {
            try
            {
                string settingsLocation = ConfigurationManager.AppSettings.Get("TestSettingsLocation");
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
