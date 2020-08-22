using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Initializers
{
    public class ContextInitializerMigrate<TDbContext> : IDbContextInitializer<TDbContext>
         where TDbContext : DbContext
    {
  
        public async Task InitializeAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeSchemaAsync(context, cancellationToken);
        }

        public async Task InitializeSchemaAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var script = context.Database.GenerateMigrationScript();

            //Can only be used for sqlserver and sqlite. Throws exception for InMemory
            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}
