using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class MultilineTextAttribute : Attribute, IDisplayMetadataAttribute
    {
        public int Rows { get; set; }
        public Boolean HTML { get; set; }

        public MultilineTextAttribute()
        {
            Rows = 7;
            HTML = false;
        }

        public MultilineTextAttribute(int rows)
        {
            Rows = rows;
        }
        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.DataTypeName = "MultilineText";
            modelMetadata.AdditionalValues["MultilineTextRows"] = Rows;
            modelMetadata.AdditionalValues["MultilineTextHTML"] = HTML;
        }
    }
}
