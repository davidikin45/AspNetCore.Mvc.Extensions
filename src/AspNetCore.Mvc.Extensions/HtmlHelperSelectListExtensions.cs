using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperSelectListExtensions
    {
        public static IHtmlContent DropDownListFromSelectList(this IHtmlHelper htmlHelper, string propertyName, object htmlAttributes = null)
        {
            var items = htmlHelper.SelectList(propertyName);

            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
            Type propertyType = GetNonNullableModelType(metadata);

            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                return htmlHelper.ListBox(propertyName, items, htmlAttributes);
            }
            else
            {
                return htmlHelper.DropDownList(propertyName, items, htmlAttributes);
            }
        }

        public static IHtmlContent CheckboxListFromSelectList(this IHtmlHelper htmlHelper, string propertyName, object htmlAttributes = null)
        {
            var items = htmlHelper.SelectList(propertyName);

            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
            Type propertyType = GetNonNullableModelType(metadata);

            bool inline = false;
            if (metadata.AdditionalValues.ContainsKey("ModelCheckboxOrRadioInline"))
            {
                inline = ((bool)metadata.AdditionalValues["ModelCheckboxOrRadioInline"]);
            }

            var sb = new StringBuilder();
            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                return htmlHelper.ValueCheckboxList(propertyName, items, inline);
            }
            else
            {
                return htmlHelper.ValueRadioList(propertyName, items, inline);
            }
        }

        public static IHtmlContent CheckboxButtonsFromSelectList(this IHtmlHelper htmlHelper, string propertyName, bool groupRadioButtons, object htmlAttributes = null, object labelCheckboxHtmlAttributes = null, object labelRadioHtmlAttributes = null)
        {
            var items = htmlHelper.SelectList(propertyName);

            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
            Type propertyType = GetNonNullableModelType(metadata);

            var sb = new StringBuilder();
            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                return htmlHelper.ValueCheckboxButtonList(propertyName, items, htmlAttributes, labelCheckboxHtmlAttributes);
            }
            else
            {
                return htmlHelper.ValueRadioButtonList(propertyName, items, groupRadioButtons, htmlAttributes, labelRadioHtmlAttributes);
            }
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
