using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public interface IValidationConventionFilter
    {
        void TransformMetadata(ValidationMetadataProviderContext context);
    }
}
