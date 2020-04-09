using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using EntityFrameworkCore.Initialization;

namespace AspNetCore.Mvc.Extensions.Data.Configuration
{
    public static class EntityFrameworkExtensions
    {
        public static IConfigurationBuilder AddEFConfiguration<TDbContext>(this IConfigurationBuilder builder, 
            Action<DbContextOptionsBuilder> optionsAction, bool initializeSchema = false)
             where TDbContext : DbContext, IConfigurationDbContext
        {
            return builder.Add(new EntityFrameworkConfigurationSource<TDbContext>(optionsAction));
        }

        public static IConfigurationBuilder AddEFConfiguration<TDbContext>(this IConfigurationBuilder builder, string connectionString, bool initializeSchema = false)
             where TDbContext : DbContext, IConfigurationDbContext
        {
            return builder.Add(new EntityFrameworkConfigurationSource<TDbContext>(optionsBuilder => {
                optionsBuilder.SetConnectionString<TDbContext>(connectionString);
            }));
        }
    }
}
