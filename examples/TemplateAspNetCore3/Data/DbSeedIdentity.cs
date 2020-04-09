using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data
{
    public class DbSeedIdentity : DbSeedIdentity<User>
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        public DbSeedIdentity(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public override IEnumerable<SeedRole> GetRolePermissions(DbContextIdentityBase<User> context)
        {
            //To allow anonymous need to add method here AND add AllowAnonymousAttribute
            return new List<SeedRole>()
            {
               new SeedRole(Role.anonymous.ToString(),
               ResourceCollectionsCore.Auth.Scopes.Authenticate,
               ResourceCollectionsCore.Auth.Scopes.ForgotPassword,
               ResourceCollectionsCore.Auth.Scopes.ResetPassword,
               ResourceCollectionsCore.Admin.Scopes.Read
               ),
               new SeedRole(Role.read_only.ToString(),
               ResourceCollectionsCore.Admin.Scopes.Read
               ),
               new SeedRole(Role.admin.ToString(),
               ResourceCollectionsCore.Admin.Scopes.Full
               )
            };
        }

        public override IEnumerable<SeedUser> GetUserRoles(DbContextIdentityBase<User> context)
        {
            return new List<SeedUser>()
            {
                 new SeedUser(User.CreateUser(_passwordHasher, "admin", "admin@admin.com", "password"), true, Role.admin.ToString())
            };
        }
    }
}
