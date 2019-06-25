using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class AppendAsterixToRequiredFieldLabels : IDisplayConventionFilter
    {
        public Func<ViewContext, bool> LimitConvention { get; }
        public AppendAsterixToRequiredFieldLabels()
            :this(((actionContext) => true))
        {

        }

        public AppendAsterixToRequiredFieldLabels(Func<ViewContext, bool> limitConvention)
        {
            LimitConvention = limitConvention;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
          
        }
    }
}
