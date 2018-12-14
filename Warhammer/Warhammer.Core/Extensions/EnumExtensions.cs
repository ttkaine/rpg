using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Warhammer.Core.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<TAttribute>();
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetAttribute<DisplayAttribute>();
            if (attribute != null)
            {
                return attribute.Name;
            }

            return enumValue.ToString();
        }

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
