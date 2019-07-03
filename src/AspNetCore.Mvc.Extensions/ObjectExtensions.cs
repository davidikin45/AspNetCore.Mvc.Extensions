using System;
using System.Linq;

namespace AspNetCore.Mvc.Extensions
{
    public static class ObjectExtensions
    {
        public static bool HasProperty(this object obj, string propName)
        {
            return obj.GetType().GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        public static object GetPropValue(this object obj, string propName)
        {
            if (HasProperty(obj, propName))
            {
                return obj.GetType().GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).GetValue(obj, null);
            }
            return null;
        }

        public static bool DynamicHasProperty(dynamic obj, string propName)
        {
            Type typeOfDynamic = obj.GetType();
            return typeOfDynamic.GetProperties().Where(p => p.Name.ToUpper().Equals(propName.ToUpper())).Any();
        }
    }
}
