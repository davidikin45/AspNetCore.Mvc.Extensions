using AspNetCore.Mvc.Extensions.Domain;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TemplateAspNetCore3.Models
{
    public class User : IdentityUser, IEntityTenant
    {
        public override string UserName
        {
            get
            {
                return Id;
            }
            set
            {

            }
        }

        private readonly List<Role> _list = new List<Role>();
        public virtual IReadOnlyList<Role> List => _list.AsReadOnly();

        public string Name { get; set; }

        public virtual ICollection<IdentityUserRole<string>> Roles { get; } = new List<IdentityUserRole<string>>();

        public static User CreateUser(IPasswordHasher<User> passwordHasher, string name, string email, string password)
        {
            var user = new User();
            user.Name = name;
            user.Email = email;
            user.EmailConfirmed = true;
            user.LockoutEnabled = true;
            user.SecurityStamp = "InitialSecurityStamp";
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            return user;
        }
    }
}
