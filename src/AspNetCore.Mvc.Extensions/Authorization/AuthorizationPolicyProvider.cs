using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthorizationOptions _options;

        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IHttpContextAccessor httpContextAccessor) : base(options)
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
