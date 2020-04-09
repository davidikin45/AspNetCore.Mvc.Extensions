using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace AspNetCore.Mvc.Extensions.Data.Configuration
{
    public class EntityFrameworkConfigurationSource<TDbContext> : IConfigurationSource
    where TDbContext : DbContext, IConfigurationDbContext
    {
        private readonly Action<DbContextOptionsBuilder> _optionsAction;
        private readonly bool _initializeSchema;

        public EntityFrameworkConfigurationSource(Action<DbContextOptionsBuilder> optionsAction,  bool initializeSchema = false)
        {
            _optionsAction = optionsAction;
            _initializeSchema = initializeSchema;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EntityFrameworkConfigurationProvider<TDbContext>(_optionsAction, _initializeSchema);
        }
    }
}
