using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;

namespace AspNetCore.Mvc.Extensions
{
    public static class ModelHelperExtensions
    {

        //Model Type
        public static Type ModelType(this ViewDataDictionary viewData)
        {
            return ModelType(viewData.Model);
        }

        public static Type ModelType(this object model)
        {
            var type = model.GetType();
            var ienum = type.GetInterface(typeof(IEnumerable<>).Name);
            type = ienum != null
              ? ienum.GetGenericArguments()[0]
              : type;
            return type;
        }

        //ModelMetadata
        public static ModelMetadata ModelMetadata(this ViewDataDictionary viewData, IModelMetadataProvider modelMetadataProvider)
        {
            return ModelMetadata(viewData.Model, modelMetadataProvider);
        }

        public static ModelMetadata ModelMetadata(this ViewDataDictionary viewData, object model, IModelMetadataProvider modelMetadataProvider)
        {
            return ModelMetadata(model, modelMetadataProvider);
        }

        public static ModelMetadata ModelMetadata(this object model, IModelMetadataProvider modelMetadataProvider)
        {
            var type = ModelType(model);
            var modelMetaData = modelMetadataProvider.GetMetadataForType(type);
            return modelMetaData;
        }

        //Labels
        public static HtmlString DisplayName(this IHtmlHelper html, object model, string propertyName)
        {
            var provider = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            return DisplayName(model, propertyName, provider);
        }

        public static String EnumDisplayName(this object e)
        {
            FieldInfo fieldInfo = e.GetType().GetField(e.ToString());
            DisplayAttribute[] displayAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return null != displayAttributes && displayAttributes.Length > 0 ? displayAttributes[0].Name : e.ToString();
        }

        public static HtmlString DisplayName(this object model, string propertyName, IModelMetadataProvider modelMetadataProvider)
        {
            Type type = ModelType(model);
            var modelMetadata = modelMetadataProvider.GetMetadataForProperty(type, propertyName);
            var value = modelMetadata.DisplayName ?? modelMetadata.PropertyName;
            return new HtmlString(HtmlEncoder.Default.Encode(value));
        }

        //Display
        public static IHtmlContent Display<T>(this IHtmlHelper html, T model, string propertyName)
        {
            IHtmlHelper<T> newHtml = html.For<T>(model);
            return newHtml.Display(propertyName);
        }

        public static IHtmlContent Display(this IHtmlHelper html, dynamic model, string propertyName)
        {
            IHtmlHelper<dynamic> newHtml = HtmlHelperExtensions.For(html, model);
            return newHtml.Display(propertyName);
        }

        //Values
        public static HtmlString DisplayTextSimple(this IHtmlHelper html, string propertyName)
        {
            return DisplayTextSimple(html, html.ViewData.Model, propertyName);
        }

        public static HtmlString DisplayTextSimple(this IHtmlHelper html, object model, string propertyName)
        {
            var newViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };

            var modelExporer = ExpressionMetadataProvider.FromStringExpression(propertyName, newViewData, html.MetadataProvider);

            string value = "";

            if (modelExporer != null)
            {
                value = modelExporer.GetSimpleDisplayText() ?? string.Empty;
                //if (modelExporer.Metadata.HtmlEncode)
                //{
                //    value = HtmlEncoder.Default.Encode(value);
                //}

            }

            return new HtmlString(value);
        }

        public static HtmlString DisplayFormatString(this object model, string propertyName, IModelMetadataProvider modelMetadataProvider)
        {
            Type type = ModelType(model);
            var modelMetadata = modelMetadataProvider.GetMetadataForType(type);

            var propertyMetadata = (from p in modelMetadata.Properties
                                    where p.PropertyName == propertyName
                                    select p).FirstOrDefault<Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata>();
            string value = "";

            if (propertyMetadata != null)
            {
                value = propertyMetadata.DisplayFormatString;
                if (propertyMetadata.HtmlEncode)
                {
                    value = HtmlEncoder.Default.Encode(value);
                }
            }

            return new HtmlString(value);
        }

    }
}

