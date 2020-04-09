using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authorization
{
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
                var scopes = policyName.Split(',').Select(p => p.Trim()).ToList();

                policy = new AuthorizationPolicyBuilder().RequireClaim("scope", scopes).Build();

                _options.AddPolicy(policyName, policy);
            }

            return policy;
        }
    }
}
