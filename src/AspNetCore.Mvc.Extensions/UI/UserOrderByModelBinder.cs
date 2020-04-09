using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.UI
{
    //
    public class UserOrderByModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(OrderBySpecification<>) || context.Metadata.ModelType == typeof(UserOrderBySpecification<>))
            {
                return new BinderTypeModelBinder(typeof(UserOrderByModelBinder));
            }

            return null;
        }
    }

    public class UserOrderByModelBinder : IModelBinder
    {
        public UserOrderByModelBinder()
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

            OrderBySpecification spec = null;
            if (valueProviderResult == ValueProviderResult.None)
            {
                spec = UserOrderBySpecification.Create(type, "");
            }
            else
            {
                bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
                var value = valueProviderResult.FirstValue;
                spec = UserOrderBySpecification.Create(type, value);
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
