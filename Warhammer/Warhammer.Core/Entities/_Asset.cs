using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Warhammer.Core.Helpers;

namespace Warhammer.Core.Entities
{
    public partial class Asset
    {
        public bool Delete { get; set; }
        public string UpkeepDisplay => CurrencyHelper.DisplayString(Upkeep);
        public bool ShowUpkeep { get; set; }

        [AllowHtml]
        public string FormattedContent
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return "";
                }

                string text = Description;
                text = Regex.Replace(text, @"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", "<a href=\"$&\" target=\"_blank\">$&</a>");
                text = Regex.Replace(text, @"<a href=""(?!http)", @"<a href=""http://"); // If the URL doesn't start with http, make it so it does :)
                text = text.Replace(Environment.NewLine, "<br/>");
                return text;
            }
        }
    }
}