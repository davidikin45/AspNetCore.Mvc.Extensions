using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public interface IValidationMetadataConfigurator
    {
        void Configure(ValidationMetadata metadata);
    }
}