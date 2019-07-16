using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace AspNetCore.Base.ModelBinders
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromFormRouteQueryAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        /// <inheritdoc />
        public BindingSource BindingSource => BindingSource.ModelBinding;

        /// <inheritdoc />
        public string Name { get; set; }
    }
}
