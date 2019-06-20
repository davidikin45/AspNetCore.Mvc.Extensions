using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class HelpTextAttribute : Attribute, IDisplayMetadataAttribute
    {
        public string HelpText { get; set; }

        public HelpTextAttribute(string helpText)
        {
            HelpText = helpText;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["HelpText"] = HelpText;
        }
    }
}
