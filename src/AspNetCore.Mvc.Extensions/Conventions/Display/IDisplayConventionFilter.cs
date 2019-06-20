using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public interface IDisplayConventionFilter
    {
        void TransformMetadata(DisplayMetadataProviderContext context);
    }
}
