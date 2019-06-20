using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class TextboxPlaceholderConventionFilter : IDisplayConventionFilter
    {
        public TextboxPlaceholderConventionFilter()
        {

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
                  string.IsNullOrEmpty(placeholder))
            {
                context.DisplayMetadata.Placeholder = () => displayName + "...";
            }
        }
    }
}