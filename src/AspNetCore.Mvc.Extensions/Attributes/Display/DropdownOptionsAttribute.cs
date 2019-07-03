using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class DropdownOptionsAttribute : SelectListOptionsAttribute, IDisplayMetadataAttribute
    {
        public DropdownOptionsAttribute(params object[] values)
            :base(values)
        {
          
        }

        public DropdownOptionsAttribute(object[] text, object[] values)
           : base(text, values)
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelDropdown";
        }
    }
}
