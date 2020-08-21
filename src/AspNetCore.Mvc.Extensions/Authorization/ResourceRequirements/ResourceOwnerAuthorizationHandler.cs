using AspNetCore.Mvc.Extensions.Domain;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, IEntityOwned>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       OperationAuthorizationRequirement requirement,
                                                       IEntityOwned entity)
        {
            //if the user has full access they can access entity
            if (context.User.Claims.Where(c => c.Type == JwtClaimTypes.Scope && c.Value == ResourceCollectionsCore.Admin.Scopes.Full).Count() > 0)
            {
                context.Succeed(requirement);
            }
            else if (!requirement.Name.Contains("-if-owner") && context.User.Claims.Where(c => c.Type == JwtClaimTypes.Scope && requirement.Name.Split(',').Contains(c.Value)).Count() > 0)
            {
                context.Succeed(requirement);
            }
            else if (requirement.Name.Contains("-if-owner") && context.User.Claims.Where(c => c.Type == JwtClaimTypes.Scope && requirement.Name.Split(',').Contains(c.Value)).Count() > 0)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (entity.OwnedBy == userId)
                {
                    context.Succeed(requirement);
                }else if(entity.OwnedBy == null && requirement.Name.Contains("read"))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
