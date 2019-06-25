using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class OffsetRightAttribute : Attribute, IDisplayMetadataAttribute
    {
        public int OffSetColumns { get; set; } = 0;

        public OffsetRightAttribute(int offSetColumns)
        {
            OffSetColumns = offSetColumns;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["OffsetRight"] = OffSetColumns;
        }
    }
}
