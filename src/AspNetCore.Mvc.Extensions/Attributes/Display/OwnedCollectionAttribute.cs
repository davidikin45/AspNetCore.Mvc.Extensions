using AspNetCore.Mvc.SelectList;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Composition Properties (1-To-Many, child cannot exist independent of the parent) 
    public class OwnedCollectionAttribute : SelectListOwnedCollectionAttribute, IDisplayMetadataAttribute
    {
        public OwnedCollectionAttribute(string dataTextFieldExpression)
        {
            DataTextFieldExpression = dataTextFieldExpression;
            DataValueField = "Id";
        }

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.TemplateHint = "ModelOwnedCollection";
        }
    }
}
