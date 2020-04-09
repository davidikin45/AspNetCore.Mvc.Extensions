using AspNetCore.Mvc.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data
{
    //https://www.meziantou.net/2017/09/11/testing-ef-core-in-memory-using-sqlite
    public class SqliteInMemoryDbContextFactory<TDbContext> : SqliteInMemoryConnectionFactory
        where TDbContext : DbContext
    {
        private readonly Action<String> _logger;
        private readonly Func<DbContextOptions<TDbContext>, TDbContext> _factory;
        public SqliteInMemoryDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> factory)
        {
            _factory = factory;
        }
        public SqliteInMemoryDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> factory, Action<String> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        private ILoggerFactory CommandLoggerFactory(Action<string> logger)
         => new ServiceCollection().AddLogging(builder =>
         {
             builder.AddAction(logger).AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
         }).BuildServiceProvider()
         .GetService<ILoggerFactory>();

        private bool _created = false;
        private DbContextOptions<TDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<TDbContext>()
                .UseSqlite(_connection)
                .UseLoggerFactory(CommandLoggerFactory(_logger))
                .EnableSensitiveDataLogging()
                .Options;
        }

        //cant create and seed using the same context
        public async Task<TDbContext> CreateContextAsync(bool create = true, CancellationToken cancellationToken = default)
        {
            await GetConnection(cancellationToken);

            if (!_created && create)
            {
                using (var context = _factory(CreateOptions()))
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                }
                _created = true;
            }

            return _factory(CreateOptions());
        }
    }
}
