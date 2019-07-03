using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperEnumExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectList(Type t, string selectedValue = null)
        {
            var dictionary = ToDictionary(t);
            return dictionary.Select(kvp => new SelectListItem() { Text = kvp.Value, Value = kvp.Key, Selected = selectedValue != null && kvp.Key == selectedValue });
        }

        public static Dictionary<string, string> ToDictionary(Type t)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (FieldInfo field in t.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                string description = field.Name;
                string id = field.Name;

                foreach (DisplayAttribute displayAttribute in field.GetCustomAttributes(true).OfType<DisplayAttribute>())
                {
                    description = displayAttribute.Name;
                }

                dictionary.Add(id, description);
            }

            return dictionary;
        }

        public static IEnumerable<SelectListItem> ToSelectList<T>(string selectedValue = null)
        {
            return ToSelectList(typeof(T), selectedValue);
        }

        public static Dictionary<string, string> ToDictionary<T>()
        {
            return ToDictionary(typeof(T));
        }
        public static IHtmlContent EnumDropDownListForStringValue<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes = null)
        {
            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider);
            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = modelExplorer.Metadata;

            Type enumType = GetNonNullableModelType(metadata);
            Type baseEnumType = Enum.GetUnderlyingType(enumType);

            var items = ToSelectList(enumType).ToList();

            if (metadata.IsNullableValueType)
            {
                items.Insert(0, new SelectListItem { Text = "", Value = "" });
            }

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }

        private static Type GetNonNullableModelType(Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;
            Type underlyingType = Nullable.GetUnderlyingType(realModelType);


            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }

            return realModelType;
        }

    }
}
