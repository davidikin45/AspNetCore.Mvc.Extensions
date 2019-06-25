using AspNetCore.Mvc.Extensions.Attributes.Display;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Providers
{
    public class AttributeMetadataProvider : IDisplayMetadataProvider
    {

        private readonly IServiceProvider _serviceProvider;
        public AttributeMetadataProvider(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {

            if (context.PropertyAttributes != null)
            {
                foreach (object propAttr in context.PropertyAttributes)
                {
                    if (propAttr is IDisplayMetadataAttribute)
                    {
                        ((IDisplayMetadataAttribute)propAttr).TransformMetadata(context, _serviceProvider);
                    }
                }
            }
        }
    }
}
