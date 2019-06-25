using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class TextboxPlaceholderConventionFilter : IDisplayConventionFilter
    {
        private readonly Func<DisplayMetadataProviderContext, bool> _limitConvention;
        public TextboxPlaceholderConventionFilter()
            : this((context) => true)
        {

        }

        public TextboxPlaceholderConventionFilter(Func<DisplayMetadataProviderContext, bool> limitConvention)
        {
            _limitConvention = limitConvention;
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
                  string.IsNullOrEmpty(placeholder) && _limitConvention(context))
            {
                context.DisplayMetadata.Placeholder = () => displayName + "...";
            }
        }
    }
}