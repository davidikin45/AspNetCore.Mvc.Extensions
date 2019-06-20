using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class TextAreaByNameConventionFilter : IDisplayConventionFilter
    {
        public TextAreaByNameConventionFilter()
        {
        }

        private static readonly HashSet<string> TextAreaFieldNames =
				new HashSet<string>
						{
							"body",
                            "comments"
                        };

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            if (!string.IsNullOrEmpty(propertyName) &&
                string.IsNullOrEmpty(modelMetadata.DataTypeName) &&
                TextAreaFieldNames.Any(propertyName.ToLower().Contains))
            {
                modelMetadata.DataTypeName = "MultilineText";
                modelMetadata.AdditionalValues["MultilineTextRows"] = 7;
                modelMetadata.AdditionalValues["MultilineTextHTML"] = false;
            }
        }
    }
}