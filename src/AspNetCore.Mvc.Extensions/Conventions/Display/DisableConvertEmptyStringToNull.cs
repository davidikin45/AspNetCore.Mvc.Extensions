using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class DisableConvertEmptyStringToNull : IDisplayConventionFilter
    {
        private readonly Func<DisplayMetadataProviderContext, bool> _applyConvention;
        public DisableConvertEmptyStringToNull()
            :this((context) => true)
        {

        }

        public DisableConvertEmptyStringToNull(Func<DisplayMetadataProviderContext, bool> applyConvention)
        {
            _applyConvention = applyConvention;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind == ModelMetadataKind.Property && _applyConvention(context))
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
