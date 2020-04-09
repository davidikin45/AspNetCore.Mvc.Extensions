using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data.Configuration
{
    public class EntityFrameworkConfigurationProvider<TDbContext> : ConfigurationProvider
    where TDbContext : DbContext, IConfigurationDbContext
    {
        private readonly bool _initializeSchema;

        public EntityFrameworkConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction, bool initializeSchema = false)
        {
            OptionsAction = optionsAction;
            _initializeSchema = initializeSchema;
        }

        public Action<DbContextOptionsBuilder> OptionsAction { get; }
        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<TDbContext>();

            OptionsAction(builder);

            using (var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), builder.Options))
            {
                if(_initializeSchema)
                    context.EnsureDbAndTablesCreatedAsync().Wait();

                Data = context.ConfigurationEntries.Any()
                    ? context.ConfigurationEntries.ToDictionary(c => c.Key, c => c.Value)
                    : new Dictionary<string, string>();
            }
        }
    }
}
