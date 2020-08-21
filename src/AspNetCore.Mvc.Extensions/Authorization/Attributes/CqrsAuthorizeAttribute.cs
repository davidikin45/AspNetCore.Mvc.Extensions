using AspNetCore.Cqrs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class CqrsAuthorizeAttribute : TypeFilterAttribute
    {
        public CqrsAuthorizeAttribute()
            : base(typeof(AuthorizeOperationFilter))
        {
            Arguments = new object[] {  };
        }

        private class AuthorizeOperationFilter : IAsyncActionFilter
        {
            private readonly IAuthorizationService _authorizationService;

            public AuthorizeOperationFilter(IAuthorizationService authorizationService)
            {
                _authorizationService = authorizationService;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var controllerActionDescriptor = (context.ActionDescriptor as ControllerActionDescriptor);

                var anonymousAction = controllerActionDescriptor?.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().FirstOrDefault();

                if (anonymousAction == null)
                {
                    var success = false;

                    if (context.ActionArguments.TryGetValue("action", out object value) && value is ActionDto action)
                    {
                        var authorizationResult = await _authorizationService.AuthorizeAsync(context.HttpContext.User, action.Type);
                        if (authorizationResult.Succeeded)
                        {
                            success = true;
                        }
                    }

                    if (context.ActionArguments.TryGetValue("type", out object valueString) && valueString is string type)
                    {
                        var authorizationResult = await _authorizationService.AuthorizeAsync(context.HttpContext.User, type);
                        if (authorizationResult.Succeeded)
                        {
                            success = true;
                        }
                    }

                    if (!success)
                    {
                        if (context.HttpContext.User.Identity.IsAuthenticated)
                        {
                            //403
                            context.Result = new ForbidResult();
                        }
                        else
                        {
                            //401
                            context.Result = new ChallengeResult();
                        }

                        return;
                    }
                }

                await next();
            }
        }
    }

}