using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class DropdownDbContextAttribute : SelectListDbAttribute, IDisplayMetadataAttribute
    {
        public DropdownDbContextAttribute(Type dbContextType, Type modelType)
            :base(dbContextType, modelType)
        {
            
        }

        public DropdownDbContextAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression)
              : base(dbContextType, modelType, dataTextFieldExpression)
        {

        }

        public DropdownDbContextAttribute(Type dbContextType, Type modelType, string dataTextFieldExpression, string dataValueField)
              : base(dbContextType, modelType, dataTextFieldExpression)
        {
            DataValueField = dataValueField;
        }

        public DropdownDbContextAttribute(string dbContextType, Type modelType)
            : base(Type.GetType(dbContextType), modelType)
        {

        }

        public DropdownDbContextAttribute(string dbContextType, Type modelType, string dataTextFieldExpression)
              : base(Type.GetType(dbContextType), modelType, dataTextFieldExpression)
        {

        }

        public DropdownDbContextAttribute(string dbContextType, Type modelType, string dataTextFieldExpression, string dataValueField)
              : base(Type.GetType(dbContextType), modelType, dataTextFieldExpression)
        {
            DataValueField = dataValueField;
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
