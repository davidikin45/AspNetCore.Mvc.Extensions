using AspNetCore.Mvc.Extensions.Conventions.Display;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Providers
{
    public class ValidationConventionsMetadataProvider : IValidationMetadataProvider
    {
        private readonly IValidationConventionFilter[] _validationMetadataFilters;

        public ValidationConventionsMetadataProvider(IValidationConventionFilter[]  validationMetadataFilters)
        {
            _validationMetadataFilters = validationMetadataFilters;
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            Array.ForEach(_validationMetadataFilters, m => m.TransformMetadata(context));
        }
    }
}
