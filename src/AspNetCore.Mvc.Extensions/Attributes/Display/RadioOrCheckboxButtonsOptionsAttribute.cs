using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class RadioOrCheckboxButtonsOptionsAttribute : SelectListOptionsAttribute, IDisplayMetadataAttribute
    {
        public bool Checkbox {get; set; } = false;
        public bool Inline { get; set; } = true;
        public bool Group { get; set; } = false;

        public RadioOrCheckboxButtonsOptionsAttribute(params object[] text)
            :base(text)
        {

        }

        public RadioOrCheckboxButtonsOptionsAttribute(object[] text, object[] values)
           : base(text, values)
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelRadioOrCheckboxButtonList";
            modelMetadata.AdditionalValues["ModelRadioCheckboxInline"] = Inline;
            modelMetadata.AdditionalValues["ModelRadioOrCheckboxGroup"] = Group;
            modelMetadata.AdditionalValues["Checkbox"] = Checkbox;
        }
    }
}
