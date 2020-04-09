using AspNetCore.Mvc.Extensions.Data;
using Microsoft.EntityFrameworkCore;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data
{
    public class IdentityContext : DbContextIdentityBase<User>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options = null) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //https://stackoverflow.com/questions/47767267/ef-core-2-how-to-include-roles-navigation-property-on-identityuser
            builder.Entity<User>()
               .HasMany(e => e.Roles)
               .WithOne()
               .HasForeignKey(e => e.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        }

        public override void BuildQueries(ModelBuilder builder)
        {

        }
    }
}
