using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public interface IModelMetadataConfiguration
    {
        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <value>The type of the model.</value>
        Type ModelType
        {
            get;
        }

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        /// <value>The configurations.</value>
        IDictionary<ModelMetadataIdentity, IMetadataConfigurator> MetadataConfigurators
        {
            get;
        }


    }
}