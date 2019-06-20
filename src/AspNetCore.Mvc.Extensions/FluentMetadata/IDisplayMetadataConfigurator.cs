using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public interface IDisplayMetadataConfigurator
    {
        void Configure(DisplayMetadata metadata);
    }
}