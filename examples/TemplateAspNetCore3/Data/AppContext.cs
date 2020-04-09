using AspNetCore.Mvc.Extensions.Data;
using Microsoft.EntityFrameworkCore;
using TemplateAspNetCore3.Data.Configuration;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data
{
    public class AppContext : DbContextBase
    {
        public DbSet<Author> Authors { get; set; }

        public AppContext()
        {

        }

        public AppContext(DbContextOptions<AppContext> options = null) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new AuthorConfiguration());
        }

        public override void BuildQueries(ModelBuilder builder)
        {

        }

        public override void Seed()
        {
            DbSeed.Seed(this);
        }
    }
}
