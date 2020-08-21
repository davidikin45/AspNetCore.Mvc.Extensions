using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class ResourceAuthorizeAttribute : TypeFilterAttribute
    {
        public ResourceAuthorizeAttribute(params string[] operations)
            : base(typeof(AuthorizeOperationFilter))
        {
            Arguments = new object[] { operations };
        }

        private class AuthorizeOperationFilter : IAsyncActionFilter
        {
            private readonly IAuthorizationService _authorizationService;
            private readonly string[] _operations;

            public AuthorizeOperationFilter(IAuthorizationService authorizationService, string[] operations)
            {
                _authorizationService = authorizationService;
                _operations = operations;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                string collectionId = null;
                var resourceAttribute = (ResourceCollectionAttribute)context.Controller.GetType().GetCustomAttributes(typeof(ResourceCollectionAttribute), true).FirstOrDefault();
                if (resourceAttribute != null)
                {
                    collectionId = resourceAttribute.CollectionId;
                }

                var controllerActionDescriptor = (context.ActionDescriptor as ControllerActionDescriptor);

                var anonymousAction = controllerActionDescriptor?.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().FirstOrDefault();

                if (anonymousAction == null)
                {
                    var success = false;
                    foreach (var operation in _operations)
                    {
                        string resourceOperation = operation;

                        if (!string.IsNullOrWhiteSpace(collectionId))
                        {
                            //e.g must have update or collectionId.update
                            resourceOperation = resourceOperation + "," + collectionId + "." + operation;
                        }

                        //Calls the AuthorizationPolicyProvider
                        var authorizationResult = await _authorizationService.AuthorizeAsync(context.HttpContext.User, resourceOperation);
                        if (authorizationResult.Succeeded)
                        {
                            success = true;
                            break;
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