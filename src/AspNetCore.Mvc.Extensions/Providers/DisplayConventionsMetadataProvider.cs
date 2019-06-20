using AspNetCore.Mvc.Extensions.Conventions.Display;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Providers
{
    public class DisplayConventionsMetadataProvider : IDisplayMetadataProvider
    {
        private readonly IDisplayConventionFilter[] _displayMetadataFilters;

        public DisplayConventionsMetadataProvider(
            IDisplayConventionFilter[] displayMetadataFilters)
        {
            _displayMetadataFilters = displayMetadataFilters;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            Array.ForEach(_displayMetadataFilters, m => m.TransformMetadata(context));
        }
    }
}
