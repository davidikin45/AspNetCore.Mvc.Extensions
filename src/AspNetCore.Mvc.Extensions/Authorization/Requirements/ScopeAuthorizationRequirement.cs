using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Authorization.Requirements
{
    public class ScopeAuthorizationRequirement : ClaimsAuthorizationRequirement
    {
        public ScopeAuthorizationRequirement(IEnumerable<string> allowedValues)
            :base("scope", allowedValues)
        {

        }
    }
}
