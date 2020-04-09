using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.UI
{
    public class UserFilterModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(FilterSpecification<>) || context.Metadata.ModelType == typeof(UserFilterSpecification<>))
            {
                return new BinderTypeModelBinder(typeof(UserFilterModelBinder));
            }

            return null;
        }
    }

    public class UserFilterModelBinder : IModelBinder
    {
        public UserFilterModelBinder()
        {
         
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var type = bindingContext.ModelType.GetGenericArguments().First();

            FilterSpecification spec = UserFilterSpecification.Create(type, bindingContext.ActionContext.HttpContext.Request.Query);

            if (!spec.IsValid)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                bindingContext.ModelState.TryAddModelError("", $"One or more of the query string filters are invalid.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(spec);
            return Task.CompletedTask;
        }
    }
}
