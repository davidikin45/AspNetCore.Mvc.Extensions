using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class HtmlByNameConventionFilter : IDisplayConventionFilter
    {
        private readonly Func<DisplayMetadataProviderContext, bool> _applyConvention;
        public HtmlByNameConventionFilter()
            : this((context) => true)
        {

        }

        public HtmlByNameConventionFilter(Func<DisplayMetadataProviderContext, bool> applyConvention)
        {
            _applyConvention = applyConvention;
        }

        private static readonly HashSet<string> TextAreaFieldNames =
                new HashSet<string>
                        {
                            "html"
                        };

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            if (!string.IsNullOrEmpty(propertyName) &&
                  string.IsNullOrEmpty(modelMetadata.DataTypeName) &&
                  TextAreaFieldNames.Any(propertyName.ToLower().Contains) && _applyConvention(context))
            {
                modelMetadata.DataTypeName = "Html";
            }
        }
    }
}