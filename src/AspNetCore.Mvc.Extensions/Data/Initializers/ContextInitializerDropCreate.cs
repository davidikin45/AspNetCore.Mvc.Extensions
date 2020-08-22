using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Initializers
{
    public class ContextInitializerDropCreate<TDbContext> : IDbContextInitializer<TDbContext>
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

            //Recreate databases with the current data model. This is useful for development as no migrations are applied.
            await context.EnsureDbAndTablesCreatedAsync(cancellationToken);
        }
    }
}
