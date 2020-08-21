using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Data.Initializers;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Initializers
{
    public abstract class ContextInitializerDropMigrate<TDbContext> : IDbContextInitializer<TDbContext>
         where TDbContext : DbContext
    {
        public async Task InitializeAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeSchemaAsync(context, cancellationToken);
        }

        public async Task InitializeSchemaAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            //Delete database relating to this context only
            await context.EnsureTablesAndMigrationsDeletedAsync(cancellationToken);

            var script = context.Database.GenerateMigrationScript();

            //Can only be used for sqlserver and sqlite. Throws exception for InMemory
            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}
