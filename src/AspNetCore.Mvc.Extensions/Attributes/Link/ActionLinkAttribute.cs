using AspNetCore.Mvc.Extensions.Attributes.Display;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Link
{
    public class ActionLinkAttribute : Attribute, IDisplayMetadataAttribute
    {
        public string LinkText { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }

        public ActionLinkAttribute(string linkText, string actionName, string controllerName)
        {
            LinkText = linkText;
            ActionName = actionName;
            ControllerName = controllerName;
        }

        public ActionLinkAttribute(string actionName, string controllerName)
        {
            ActionName = actionName;
            ControllerName = controllerName;
        }

        public ActionLinkAttribute(string linkText)
        {
            LinkText = linkText;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["LinkText"] = LinkText;
            modelMetadata.AdditionalValues["ActionName"] = ActionName;
            modelMetadata.AdditionalValues["ControllerName"] = ControllerName;
        }
    }

    [AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class LinkRouteValueAttribute : Attribute, IDisplayMetadataAttribute
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public LinkRouteValueAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            RouteValueDictionary routeValueDictionary = null;

            if (modelMetadata.AdditionalValues.ContainsKey("RouteValueDictionary"))
            {
                routeValueDictionary = (RouteValueDictionary)modelMetadata.AdditionalValues["RouteValueDictionary"];
            }

            if (routeValueDictionary == null)
            {
                routeValueDictionary = new RouteValueDictionary();
            }

            routeValueDictionary.Add(Key, Value);

            modelMetadata.AdditionalValues["RouteValueDictionary"] = routeValueDictionary;
        }
    }
}
