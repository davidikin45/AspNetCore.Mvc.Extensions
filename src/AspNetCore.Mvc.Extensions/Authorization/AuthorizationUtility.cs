using System;
using System.Security.Claims;
using System.Security.Principal;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class AuthorizationUtility
    {
        private ClaimsIdentity _Identity;
        private IPrincipal _Principal;

        public AuthorizationUtility(IIdentity identity, IPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException("principal", "principal is null.");
            if (identity == null)
                throw new ArgumentNullException("identity", "identity is null.");

            _Identity = identity as ClaimsIdentity;
            _Principal = principal;
        }

        public bool IsInRole(string role)
        {
            if (_Principal == null)
            {
                return false;
            }
            else
            {
                return _Principal.IsInRole(role);
            }
        }

        public bool IsAuthorized(string permissionName, int id)
        {
            if (_Identity == null)
            {
                return false;
            }
            else
            {
                if (IsAuthorized("admin") == true)
                {
                    return true;
                }
                else
                {
                    return _Identity.HasClaim(permissionName, id.ToString());
                }
            }
        }

        public bool IsAuthorized(string roleName)
        {
            return HasClaim(ClaimTypes.Role, roleName);
        }

        public bool HasScope(string scope)
        {
            return HasClaim("scope", scope);
        }

        public bool HasClaim(string claimType, string claimValue)
        {
            if (_Identity == null)
            {
                return false;
            }
            else
            {
                return _Identity.HasClaim(claimType, claimValue);
            }
        }

        public bool HasClaim(string claimType)
        {
            if (_Identity == null)
            {
                return false;
            }
            else
            {
                return _Identity.HasClaim(c => c.Type == claimType);
            }
        }
    }
}
