using IdentityModel.AspNetCore.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization.PolicyProviders
{
    //https://auth0.com/docs/authorization/rbac/enable-role-based-access-control-for-apis
    //https://www.jerriepelser.com/blog/creating-dynamic-authorization-policies-aspnet-core/
    // Extremely useful so you don't have to create policies for each scope you wish to check for.
    // policy is a collection of requirements which must ALL be satisfied. But you can certainly have a single requirement that can succeeds in several different ways.
    public class ScopeAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;

        public ScopeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            _options = options.Value;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Check static policies first
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                var scopes = policyName.Split(',').Select(p => p.Trim()).ToArray();

                policy = new AuthorizationPolicyBuilder().RequireScope(scopes).Build();

                _options.AddPolicy(policyName, policy);
            }

            return policy;
        }
    }
}
