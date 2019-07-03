using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class CheckboxOrRadioOptionsAttribute : SelectListOptionsAttribute
    {
        public bool Inline { get; set; }

        public CheckboxOrRadioOptionsAttribute(params object[] text)
            :base(text)
        {

        }
        public CheckboxOrRadioOptionsAttribute(object[] text, object[] values)
           : base(text, values)
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelCheckboxOrRadio";
            modelMetadata.AdditionalValues["ModelCheckboxOrRadioInline"] = Inline;
        }
    }
}
