using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

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

        public bool HasInlineImage => ImageData != null && ImageData.Length > 50 && !string.IsNullOrWhiteSpace(ImageMime);

        public bool HasExternalImage
        {
            get
            {
                return PageImages.Any(p => p.IsPrimary);
            }
        }

        public bool HasImage => HasInlineImage || HasExternalImage;

        public byte[] PrimaryImage
        {
            get
            {
                if (HasExternalImage)
                {
                    PageImage image = PageImages.FirstOrDefault(p => p.IsPrimary);
                    return image?.Data;
                }
                if (HasInlineImage)
                {
                    return ImageData;
                }
                return null;
            }
        }
 
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

        public bool PlayerIsGm { get; set; }
        public bool ShowGmNotes { get; set; }
    }
}
