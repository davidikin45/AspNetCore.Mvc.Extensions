using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class DisableConvertEmptyStringToNull : IDisplayConventionFilter
    {
        public DisableConvertEmptyStringToNull()
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind == ModelMetadataKind.Property)
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
