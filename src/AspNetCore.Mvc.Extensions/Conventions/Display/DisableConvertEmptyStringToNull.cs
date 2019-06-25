using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class DisableConvertEmptyStringToNull : IDisplayConventionFilter
    {
        private readonly Func<DisplayMetadataProviderContext, bool> _limitConvention;
        public DisableConvertEmptyStringToNull()
            :this((context) => true)
        {

        }

        public DisableConvertEmptyStringToNull(Func<DisplayMetadataProviderContext, bool> limitConvention)
        {
            _limitConvention = limitConvention;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind == ModelMetadataKind.Property && _limitConvention(context))
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
