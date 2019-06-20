using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class BooleanTrueFalseButtonsAttribute : Attribute, IDisplayMetadataAttribute
    {
        public BooleanTrueFalseButtonsAttribute()
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.DataTypeName = "BooleanTrueFalseButtons";
        }
    }
}