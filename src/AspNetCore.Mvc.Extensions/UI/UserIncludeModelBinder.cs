using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.UI
{
    public class UserIncludeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(IncludeSpecification<>) || context.Metadata.ModelType == typeof(UserIncludeSpecification<>))
            {
                return new BinderTypeModelBinder(typeof(UserIncludeModelBinder));
            }

            return null;
        }
    }

    public class UserIncludeModelBinder : IModelBinder
    {
        public UserIncludeModelBinder()
        {
         
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var type = bindingContext.ModelType.GetGenericArguments().First();
            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            IncludeSpecification spec = null;
            if (valueProviderResult == ValueProviderResult.None)
            {
                spec = UserIncludeSpecification.Create(type, "");
            }
            else
            {
                bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
                var value = valueProviderResult.FirstValue;
                spec = UserIncludeSpecification.Create(type, value);
            }

            if (!spec.IsValid)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                bindingContext.ModelState.TryAddModelError(modelName, $"One or more of the {modelName} fields are invalid.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(spec);
            return Task.CompletedTask;
        }
    }
}
