using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class TextboxPlaceholderConventionFilter : IDisplayConventionFilter
    {
        private readonly Func<DisplayMetadataProviderContext, bool> _applyConvention;
        public TextboxPlaceholderConventionFilter()
            : this((context) => true)
        {

        }

        public TextboxPlaceholderConventionFilter(Func<DisplayMetadataProviderContext, bool> applyConvention)
        {
            _applyConvention = applyConvention;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;
            var displayName = "";
            if (modelMetadata.DisplayName != null)
            {
                displayName = modelMetadata.DisplayName.Invoke();
            }
            var placeholder = "";
            if (modelMetadata.Placeholder != null)
            {
                placeholder = modelMetadata.Placeholder.Invoke();
            }

            if (!string.IsNullOrEmpty(displayName) &&
                  string.IsNullOrEmpty(placeholder) && _applyConvention(context))
            {
                context.DisplayMetadata.Placeholder = () => displayName + "...";
            }
        }
    }
}