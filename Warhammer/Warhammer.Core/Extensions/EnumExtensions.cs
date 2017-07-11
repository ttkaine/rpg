using System;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Extensions
{
    public static class EnumExtensions
    {
        public static List<int> Numbers(this Enum e)
        {
            var vals = Enum.GetValues(e.GetType());

            return vals.Cast<int>().OrderByDescending(i => i).ToList();
        }

        public static List<T> ToList<T>(this Enum e)
        {
            Type enumType = e.GetType();

            List<string> values = Enum.GetNames(enumType).ToList();

            List<T> members = new List<T>();

            foreach (string name in values)
            {
                T member = (T)Enum.Parse(enumType, name);
                members.Add(member);
            }

            return members;
        }
    }
}
