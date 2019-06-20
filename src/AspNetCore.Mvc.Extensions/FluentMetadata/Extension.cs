using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    internal static class Extension
    {
        public static void AddValidationAttribute<TAttribute>(this ValidationMetadata metadata,
            TAttribute validAttribute) where TAttribute : Attribute

        {
            var attribute = metadata.ValidatorMetadata.OfType<TAttribute>().FirstOrDefault();
            if (attribute != null)
            {
                metadata.ValidatorMetadata.Remove(attribute);
            }
            metadata.ValidatorMetadata.Add(validAttribute);
        }
    }
}