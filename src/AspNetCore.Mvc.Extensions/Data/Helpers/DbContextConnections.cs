using AspNetCore.Mvc.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    //xUnit.net expose ITestOutputHelper
    public static class DbContextConnections
    {
        public static ILoggerFactory CommandLoggerFactory(Action<string> logger)
          => new ServiceCollection().AddLogging(builder =>
          {
              builder.AddAction(logger).AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
          }).BuildServiceProvider()
          .GetService<ILoggerFactory>();

        public static DbContextOptions<TContext> DbContextOptionsSqlite<TContext>(string dbName, Action<string> logAction = null)
          where TContext : DbContext
        {

            var connectionString = $"Data Source={dbName}.db;";

            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite(connectionString);
            builder.UseLoggerFactory(CommandLoggerFactory(logAction));
            builder.EnableSensitiveDataLogging();
            return builder.Options;
        }

        public static DbContextOptions<TContext> DbContextOptionsSqliteInMemory<TContext>(DbConnection connection, Action<string> logAction = null)
         where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite(connection);
            builder.UseLoggerFactory(CommandLoggerFactory(logAction));
            builder.EnableSensitiveDataLogging();
            return builder.Options;
        }

        public static DbContextOptions<TContext> DbContextOptionsSqliteInMemory<TContext>(Action<string> logAction = null)
         where TContext : DbContext
        {

            var connectionString = "DataSource=:memory:";

            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite(connectionString);
            builder.UseLoggerFactory(CommandLoggerFactory(logAction));
            builder.EnableSensitiveDataLogging();
            return builder.Options;
        }

        public static DbContextOptions<TContext> DbContextOptionsInMemory<TContext>(string dbName = "", Action<string> logAction = null)
             where TContext : DbContext
        {
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = Guid.NewGuid().ToString();
            }

            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseInMemoryDatabase(dbName);
            builder.UseLoggerFactory(CommandLoggerFactory(logAction));
            builder.EnableSensitiveDataLogging();
            return builder.Options;
        }

        public static DbContextOptions<TContext> DbContextOptionsSqlLocalDB<TContext>(string dbName, Action<string> logAction = null)
             where TContext : DbContext
        {
            var connectionString = new SqlConnectionStringBuilder()
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                InitialCatalog = dbName,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true
            }.ConnectionString;

            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlServer(connectionString);
            builder.UseLoggerFactory(CommandLoggerFactory(logAction));
            builder.EnableSensitiveDataLogging();
            return builder.Options;
        }
    }
}
