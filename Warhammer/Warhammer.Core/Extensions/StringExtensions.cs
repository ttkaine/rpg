using System;
using System.Linq;

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
    }
}