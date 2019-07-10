using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class RadioOrCheckboxDbAttribute : SelectListDbAttribute, IDisplayMetadataAttribute
    {
        public bool Checkbox { get; set; } = false;
        public bool Inline { get; set; } = true;
        public bool Group { get; set; } = false;

        public RadioOrCheckboxDbAttribute(Type dbContextType, Type modelType)
           : base(dbContextType, modelType)
        {

        }

        public RadioOrCheckboxDbAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression)
            : base(dbContextType, modelType, dataTextFieldExpression)
        {

        }

        public RadioOrCheckboxDbAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression, string dataValueField)
            : base(dbContextType, modelType, dataTextFieldExpression)
        {
            DataValueField = dataValueField;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {

            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelRadioOrCheckboxList";
            modelMetadata.AdditionalValues["ModelRadioOrCheckboxInline"] = Inline;
            modelMetadata.AdditionalValues["ModelRadioOrCheckboxGroup"] = Group;
            modelMetadata.AdditionalValues["Checkbox"] = Checkbox;
        }
    }
}
