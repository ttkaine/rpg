using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Warhammer.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToAlpha(this string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                return new String(s.Where(Char.IsLetter).ToArray());
            }
            return s;
        }

        public static string ToImageUrl(this string s)
        {
            return $"https://sendingofeight.blob.core.windows.net/images/{s}";
        }

        public static string ToDisplayFormat(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }

            string text = s;
            text = Regex.Replace(text, @"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", "<a href=\"$&\" target=\"_blank\">$&</a>");
            text = Regex.Replace(text, @"<a href=""(?!http)", @"<a href=""http://"); // If the URL doesn't start with http, make it so it does :)
            text = text.Replace(Environment.NewLine, "<br/>");
            return text;
        }
    }
}