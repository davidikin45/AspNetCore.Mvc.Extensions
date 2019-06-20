using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class ReadOnlyAttribute : Attribute, IDisplayMetadataAttribute
    {
        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            if (string.IsNullOrEmpty(modelMetadata.DataTypeName))
            {
                modelMetadata.DataTypeName = "ReadOnly";
            }
        }
    }
}
