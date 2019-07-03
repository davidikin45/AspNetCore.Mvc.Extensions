using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class CheckboxOrRadioButtonsOptionsAttribute : SelectListOptionsAttribute, IDisplayMetadataAttribute
    {
        public bool Inline { get; set; }

        public CheckboxOrRadioButtonsOptionsAttribute(params object[] text)
            :base(text)
        {

        }

        public CheckboxOrRadioButtonsOptionsAttribute(object[] text, object[] values)
           : base(text, values)
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelCheckboxOrRadioButtons";
            modelMetadata.AdditionalValues["ModelCheckboxOrRadioInline"] = Inline;
        }
    }
}
