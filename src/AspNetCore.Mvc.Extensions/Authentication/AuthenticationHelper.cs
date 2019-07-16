using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authentication
{
    public static class AuthenticationHelper
    {
        public static async Task<(IList<string> Roles, HashSet<string> Scopes)> GetRolesAndScopesAsync<TUser>(TUser user, UserManager<TUser> userManager, RoleManager<IdentityRole> roleManager)
            where TUser : class
        {
            var roles = await userManager.GetRolesAsync(user);
            var scopes = new HashSet<string>((await userManager.GetClaimsAsync(user)).Where(c => c.Type == "scope").Select(c => c.Value));

            var ownerRole = await roleManager.FindByNameAsync("authenticated");
            if (ownerRole != null)
            {
                var roleScopes = (await roleManager.GetClaimsAsync(ownerRole)).Where(c => c.Type == "scope").Select(c => c.Value).ToList();
                foreach (var scope in roleScopes)
                {
                    scopes.Add(scope);
                }
            }

            //Add role scopes.
            foreach (var roleName in roles)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleScopes = (await roleManager.GetClaimsAsync(role)).Where(c => c.Type == "scope").Select(c => c.Value).ToList();
                    foreach (var scope in roleScopes)
                    {
                        scopes.Add(scope);
                    }

                }
            }

            return (roles, scopes);
        }
    }
}
