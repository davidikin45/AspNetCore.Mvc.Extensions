using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data
{
    public abstract class DbSeedIdentity<TUser> where TUser : IdentityUser
    {
        public abstract IEnumerable<SeedRole> GetRolePermissions(DbContextIdentityBase<TUser> context);
        public abstract IEnumerable<SeedUser> GetUserRoles(DbContextIdentityBase<TUser> context);

        public class SeedUser
        {
            public TUser User { get; }
            public bool SyncRoles { get; }
            public string[] Roles { get; }

            public SeedUser(TUser user, bool syncRoles, params string[] roles)
            {
                User = user;
                SyncRoles = syncRoles;
                Roles = roles;
            }
        }

        public class SeedRole
        {
            public string Name { get; }
            public string[] Scopes { get; }

            public SeedRole(string name, params string[] scopes)
            {
                Name = name;
                Scopes = scopes;
            }
        }

        public virtual void SeedData(DbContextIdentityBase<TUser> context)
        {
            var roles = GetRolePermissions(context);

            CreateRoles(context, roles);
            CreateRoleScopes(context, roles);

            var users = GetUserRoles(context);
            CreateUsers(context, users);
        }

        protected virtual void CreateRoles(DbContextIdentityBase<TUser> context, IEnumerable<SeedRole> roles)
        {
            foreach (var role in roles)
            {
                if (context.Roles.Find(role.Name) == null)
                {
                    var identityRole = new IdentityRole() { Id = role.Name, Name = role.Name };
                    identityRole.NormalizedName = identityRole.Name.ToUpper();
                    context.Roles.Add(identityRole);
                }
            }

            var roleIds = roles.Select(r => r.Name);

            var remove = context.Roles.Where(r => !roleIds.Contains(r.Id)).ToList();
            context.Roles.RemoveRange(remove);
        }

        protected virtual void CreateRoleScopes(DbContextIdentityBase<TUser> context, IEnumerable<SeedRole> roles)
        {
            foreach (var role in roles)
            {
                foreach (var scope in role.Scopes)
                {
                    if (context.RoleClaims.Where(rc => rc.RoleId == role.Name && rc.ClaimType == "scope" && rc.ClaimValue == scope).FirstOrDefault() == null)
                    {
                        var claim = new IdentityRoleClaim<string>() { ClaimType = "scope", ClaimValue = scope, RoleId = role.Name };
                        context.RoleClaims.Add(claim);
                    }
                }
                var remove = context.RoleClaims.Where(rc => rc.RoleId == role.Name && rc.ClaimType == "scope" && !role.Scopes.Contains(rc.ClaimValue)).ToList();
                context.RoleClaims.RemoveRange(remove);
            }
        }

        protected virtual void CreateUsers(DbContextIdentityBase<TUser> context, IEnumerable<SeedUser> users)
        {
            foreach (var seedUser in users)
            {
                var dbUser = context.Users.Where(u => u.Email == seedUser.User.Email).FirstOrDefault();

                bool newUser = false;
                if (dbUser == null)
                {
                    newUser = true;
                    context.Users.Add(seedUser.User);
                    dbUser = seedUser.User;
                }

                if(newUser || seedUser.SyncRoles)
                {
                    foreach (var role in seedUser.Roles)
                    {
                        if (context.UserRoles.Where(ur => ur.UserId == dbUser.Id && ur.RoleId == role).FirstOrDefault() == null)
                        {
                            var userRole = new IdentityUserRole<string>() { UserId = dbUser.Id, RoleId = role };
                            context.UserRoles.Add(userRole);
                        }
                    }

                    var remove = context.UserRoles.Where(ur => ur.UserId == dbUser.Id && !seedUser.Roles.Contains(ur.RoleId)).ToList();
                    context.UserRoles.RemoveRange(remove);
                }
            }
        }
    }
}
