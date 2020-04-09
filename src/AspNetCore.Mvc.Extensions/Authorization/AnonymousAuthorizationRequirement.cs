using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class AnonymousAuthorizationRequirement : IAuthorizationRequirement
    {

    }

    public class AnonymousAuthorizationHandler : AuthorizationHandler<AnonymousAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AnonymousAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
