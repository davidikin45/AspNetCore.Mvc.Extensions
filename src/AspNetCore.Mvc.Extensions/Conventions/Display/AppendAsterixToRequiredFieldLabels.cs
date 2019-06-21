using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class AppendAsterixToRequiredFieldLabels : IDisplayConventionFilter
    {
        public Func<ViewContext, bool> ApplyConvention { get; }
        public AppendAsterixToRequiredFieldLabels()
            :this(((actionContext) => true))
        {

        }

        public AppendAsterixToRequiredFieldLabels(Func<ViewContext, bool> applyConvention)
        {
            ApplyConvention = applyConvention;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
          
        }
    }
}
