using AspNetCore.Mvc.Extensions.Authorization.Requirements;
using IdentityModel.AspNetCore.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization.PolicyProviders
{
    //https://stackoverflow.com/questions/48351332/oauth-scopes-and-application-roles-permissions

    //https://www.jerriepelser.com/blog/creating-dynamic-authorization-policies-aspnet-core/
    // Extremely useful so you don't have to create policies for each scope you wish to check for.
    public class ScopeAndAnonymousAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthorizationOptions _options;

        public ScopeAndAnonymousAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            //scoped
            var roleManager = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

            // Check static policies first
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                bool allowAnonymousAccess = false;

                //e.g check if anonymous allows for one of the passed scopes. full, create, read, read-if-owner, update, update-if-owner, delete, delete-if-owner, collectionId.create, collectionId.read, collectionId.read-if-owner, collectionId.update, collectionId.update-if-owner, collectionId.delete, collectionId.delete-if-owner
                var role = await roleManager.FindByNameAsync("anonymous");
                if (role != null)
                {
                    var roleScopes = (await roleManager.GetClaimsAsync(role)).Where(c => c.Type == "scope").Select(c => c.Value).ToList();
                    var scopes = policyName.Split(',').Select(p => p.Trim()).ToList();
                    if (roleScopes.Union(scopes).Count() > 0 ||
                        roleScopes.Contains(ResourceCollectionsCore.Admin.Scopes.Full))
                    {
                        allowAnonymousAccess = true;
                    }
                }

                if (allowAnonymousAccess)
                {
                    policy = new AuthorizationPolicyBuilder().AddRequirements(new AnonymousAuthorizationRequirement()).Build();
                }
                else
                {
                    //must have one or more to pass
                    var scopes = policyName.Split(',').Select(p => p.Trim()).ToList();

                    if (!scopes.Contains(ResourceCollectionsCore.Admin.Scopes.Full))
                        scopes.Add(ResourceCollectionsCore.Admin.Scopes.Full);

                    policy = new AuthorizationPolicyBuilder().RequireScope(scopes.ToArray()).Build();
                }

                // Add policy to the AuthorizationOptions, so we don't have to re-create it each time
                _options.AddPolicy(policyName, policy);
            }

            return policy;
        }

        public new Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return base.GetDefaultPolicyAsync();
        }
    }
}
