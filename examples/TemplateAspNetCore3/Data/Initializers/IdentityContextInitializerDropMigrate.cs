using AspNetCore.Mvc.Extensions.Data.Initializers;
using Microsoft.AspNetCore.Identity;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data.Initializers
{
    public class IdentityContextInitializerDropMigrate : ContextInitializerDropMigrate<IdentityContext>
    {
        private readonly IPasswordHasher<User> _passwordHasher;
        public IdentityContextInitializerDropMigrate(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public IdentityContextInitializerDropMigrate()
        {
            _passwordHasher = new PasswordHasher<User>();
        }

        public override void Seed(IdentityContext context, string tenantId)
        {
            var dbSeeder = new DbSeedIdentity(_passwordHasher);
            dbSeeder.SeedData(context);
        }
    }
}
