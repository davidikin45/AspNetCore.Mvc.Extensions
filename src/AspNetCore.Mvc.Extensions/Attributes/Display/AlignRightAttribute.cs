using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class AlignRightAttribute : Attribute, IDisplayMetadataAttribute
    {
        public bool AlignRight { get; set; } = true;

        public AlignRightAttribute()
        {
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["AlignRight"] = AlignRight;
        }
    }
}
