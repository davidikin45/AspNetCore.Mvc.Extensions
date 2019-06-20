using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public interface IMetadataConfiguratorProviderSingleton
    {
        IEnumerable<IMetadataConfigurator> GetMetadataConfigurators(ModelMetadataIdentity identity);
    }
}