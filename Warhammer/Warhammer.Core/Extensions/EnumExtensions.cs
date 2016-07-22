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
    }
}
