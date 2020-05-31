using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Warhammer.Core.Extensions;

namespace Warhammer.Core.Entities
{
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class Page
    {
        public virtual double BaseScore
        {
            get { return 0.25; }
        }

        public int CalculatedWordCount
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    return 0;
                }
                string theContent = Description.Trim();
                while (theContent.Contains("  "))
                {
                    theContent = theContent.Replace("  ", " ");
                }
                int words = theContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Count();
                return words;
            }
        }

        public double AgeInMonths
        {
            get { return DateTime.Now.Subtract(Created).Days/(365.25/12); }           
        }
        public virtual double AgeInDays
        {
            get
            {
                TimeSpan span = DateTime.Now - Created;
                double days = span.TotalDays;
                return days;
            }
        }

        public double DaysSinceModified
        {
            get
            {
                TimeSpan span = DateTime.Now - Modified;
                double days = span.TotalDays;
                return days;
            }
        }

        public bool HasImage => FileIdentifier != "default.png" && FileIdentifier != "default_character.jpg";
        public string ImageUrl => FileIdentifier.ToImageUrl();

        public string GetSummary(int length)
        {
            const string str = "...";
            string text = RawText;

            if (!string.IsNullOrWhiteSpace(text) && text.Length > length)
            {
                text = text.Substring(0, length - str.Length) + str;
            }
            return text;
        }

        public string RawText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    return string.Empty;
                }
                string text = Regex.Replace(Description, "<.*?>", string.Empty);
                return text;
            }
        }

        private List<PlayerSecret> _visibleSecrets;
        public List<PlayerSecret> VisibleSecrets
        {
            get
            {
                if (_visibleSecrets == null)
                {
                    List<PlayerSecret> secrets =
                        PlayerSecrets.Where(s => s.PlayerId == CurrentPlayerId || PlayerIsGm).ToList();
                    secrets = secrets.Where(s => !string.IsNullOrWhiteSpace(s.Details)).ToList();
                    if (!secrets.Any() && !PlayerIsGm)
                    {
                        secrets.Add(new PlayerSecret {PlayerId = CurrentPlayerId, PageId = Id});
                    }

                    _visibleSecrets = secrets;
                }

                return _visibleSecrets;
            }
            set { _visibleSecrets = value; }
        }

        public int CurrentPlayerId { get; set; }
        public bool PlayerIsGm { get; set; }
        public bool ShowGmNotes { get; set; }
        public bool ShowPlayerSecrets { get; set; }
        public bool ShowStatToggle { get; set; }
    }
}
