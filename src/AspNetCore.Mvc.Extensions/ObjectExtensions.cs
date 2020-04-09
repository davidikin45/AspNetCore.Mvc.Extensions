using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace AspNetCore.Mvc.Extensions
{
    public static class ObjectExtensions
    {

        public static string Name<T, TProp>(this T o, Expression<Func<T, TProp>> propertySelector)
        {
            MemberExpression body = (MemberExpression)propertySelector.Body;
            return body.Member.Name;
        }

        public static string GetEnumDescription(this Enum enumValue)
        {
            string enumValueAsString = enumValue.ToString();

            var type = enumValue.GetType();
            FieldInfo fieldInfo = type.GetField(enumValueAsString);
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                var attribute = (DescriptionAttribute)attributes[0];
                return attribute.Description;
            }

            return enumValueAsString;
        }

        public static T1 CopyFrom<T1, T2>(this T1 obj, T2 otherObject)
        where T1 : class
        where T2 : class
        {
            PropertyInfo[] srcFields = otherObject.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            PropertyInfo[] destFields = obj.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in srcFields)
            {
                var dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null && dest.CanWrite)
                    dest.SetValue(obj, property.GetValue(otherObject, null), null);
            }

            return obj;
        }

        public static object GetPropValue(this object obj, string propName)
        {
            if (HasProperty(obj, propName))
            {
                return obj.GetType().GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).GetValue(obj, null);
            }
            return null;
        }

        public static object DynamicGetPropValue(dynamic obj, string propName)
        {
            if (DynamicHasProperty(obj, propName))
            {
                Type typeOfDynamic = obj.GetType();
                return typeOfDynamic.GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).GetValue(obj, null);
            }
            return null;
        }

        public static void SetPropValue(this object obj, string propName, object value)
        {
            obj.GetType().GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).SetValue(obj, value);
        }

        public static bool IsCollectionProperty(this Type type, string propName)
        {
            if (HasProperty(type, propName))
            {
                return type.GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).PropertyType.IsCollection();
            }
            return false;
        }

        public static Type[] GetGenericArguments(this Type type, string propName)
        {
            if (HasProperty(type, propName))
            {
                return type.GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).PropertyType.GenericTypeArguments;
            }
            return null;
        }

        public static bool HasProperty(this Type type, string propName)
        {
            return type.GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        public static bool HasProperty(this object obj, string propName)
        {
            return obj.GetType().GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        public static bool DynamicHasProperty(dynamic obj, string propName)
        {
            Type typeOfDynamic = obj.GetType();
            return typeOfDynamic.GetProperties().Where(p => p.Name.ToUpper().Equals(propName.ToUpper())).Any();
        }

        public static PropertyInfo[] GetProperties(this object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
            if (HasField(obj, fieldName))
            {
                return obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
            }
            return null;
        }

        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
        }

        public static bool HasField(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null;
        }

        public static FieldInfo[] GetFields(this object obj)
        {
            return obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static Type GetDynamicType(dynamic obj)
        {
            return obj?.GetType();
        }

        public static Type GetCollectionItemType(object collection)
        {
            var type = collection.GetType();
            var ienum = type.GetInterface(typeof(IEnumerable<>).Name);
            return ienum != null
                ? ienum.GetGenericArguments()[0]
                : null;
        }
    }
}
