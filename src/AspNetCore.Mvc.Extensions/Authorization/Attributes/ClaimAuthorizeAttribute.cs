using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AspNetCore.Mvc.Extensions.Authorization.Attributes
{
    //https://stackoverflow.com/questions/31464359/how-do-you-create-a-custom-authorizeattribute-in-asp-net-core
    //https://auth0.com/docs/scopes/api-scopes
    //https://auth0.com/docs/applications/first-party-and-third-party-applications#first-party-applications
    //https://community.auth0.com/t/scopes-vs-permissions-confusion/30906/9
    public class ClaimAuthorizeAttribute : TypeFilterAttribute
    {
        public ClaimAuthorizeAttribute(string claimType, params string[] allowedValues) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = allowedValues.Select(claimValue => new Claim(claimType, claimValue)).ToArray<object>();
        }

        private class ClaimRequirementFilter : IAuthorizationFilter
        {
            readonly Claim[] _claims;

            public ClaimRequirementFilter(Claim[] claims)
            {
                _claims = claims;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var hasClaim = _claims.Any(claim => context.HttpContext.User.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value));
                if (!hasClaim)
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
        }
    }
}
