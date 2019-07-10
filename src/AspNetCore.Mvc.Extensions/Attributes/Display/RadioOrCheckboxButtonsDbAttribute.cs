using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class RadioOrCheckboxButtonsDbAttribute : SelectListDbAttribute, IDisplayMetadataAttribute
    {
        public bool Checkbox { get; set; } = false;
        public bool Inline { get; set; } = true;
        public bool Group { get; set; } = false;

        public RadioOrCheckboxButtonsDbAttribute(Type dbContextType, Type modelType)
            :base(dbContextType, modelType)
        {

        }

        public RadioOrCheckboxButtonsDbAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression)
            : base(dbContextType, modelType, dataTextFieldExpression)
        {

        }

        public RadioOrCheckboxButtonsDbAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression, string dataValueField)
            :base(dbContextType, modelType, dataTextFieldExpression)
        {
            DataValueField = dataValueField;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {

            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelRadioOrCheckboxButtonList";
            modelMetadata.AdditionalValues["ModelRadioOrCheckboxInline"] = Inline;
            modelMetadata.AdditionalValues["ModelRadioOrCheckboxGroup"] = Group;
            modelMetadata.AdditionalValues["Checkbox"] = Checkbox;
        }
    }
}
