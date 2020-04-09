using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DisplayAttribute>()
                    ?.Name;
        }
    }
}
