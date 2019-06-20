using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public interface IDisplayMetadataAttribute
    {
        void TransformMetadata(DisplayMetadataProviderContext context);
    }
}
