using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class ReadOnlyHiddenInputAttribute : Attribute, IDisplayMetadataAttribute
    {
        public bool ShowForEdit { get; set; }
        public bool ShowForCreate { get; set; }

        public ReadOnlyHiddenInputAttribute()
        {
            ShowForEdit = true;
            ShowForCreate = true;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["ReadOnlyHiddenInputEdit"] = ShowForEdit;
            modelMetadata.AdditionalValues["ReadOnlyHiddenInputCreate"] = ShowForCreate;
        }
    }
}
