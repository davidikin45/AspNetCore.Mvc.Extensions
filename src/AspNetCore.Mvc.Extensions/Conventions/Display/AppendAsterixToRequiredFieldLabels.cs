using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class AppendAsterixToRequiredFieldLabels : IDisplayConventionFilter
    {

        public Func<ViewContext, bool> AddAstertix { get; }
        public AppendAsterixToRequiredFieldLabels()
            :this(((actionContext) => true))
        {

        }

        public AppendAsterixToRequiredFieldLabels(Func<ViewContext, bool> addAstertix)
        {
            AddAstertix = addAstertix;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
          
        }
    }
}
