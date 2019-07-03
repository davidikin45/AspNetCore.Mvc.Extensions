﻿using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class CheckboxOrRadioButtonsDbAttribute : SelectListDbAttribute, IDisplayMetadataAttribute
    {
        public bool Inline { get; set; }

        public CheckboxOrRadioButtonsDbAttribute(Type dbContextType, Type modelType)
            :base(dbContextType, modelType)
        {

        }

        public CheckboxOrRadioButtonsDbAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression)
            : base(dbContextType, modelType, dataTextFieldExpression)
        {

        }

        public CheckboxOrRadioButtonsDbAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression, string dataValueField)
            :base(dbContextType, modelType, dataTextFieldExpression)
        {
            DataValueField = dataValueField;
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
