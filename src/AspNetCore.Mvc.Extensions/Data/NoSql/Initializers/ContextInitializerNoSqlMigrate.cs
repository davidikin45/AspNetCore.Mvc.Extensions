using EntityFrameworkCore.Initialization.NoSql;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.NoSql.Initializers
{
    public abstract class ContextInitializerNoSqlMigrate<TDbContext> : IDbContextNoSqlInitializer<TDbContext>
          where TDbContext : DbContextNoSql
    {

        public async Task InitializeAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeSchemaAsync(context, cancellationToken);
            await InitializeDataAsync(context, null, cancellationToken);
        }

        public Task InitializeSchemaAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public async Task InitializeDataAsync(TDbContext context, string tenantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Seed(context, tenantId);


            await OnSeedCompleteAsync(context);
        }

        public abstract void Seed(TDbContext context, string tenantId);
        public virtual Task OnSeedCompleteAsync(TDbContext context)
        {
            return Task.CompletedTask;
        }
    }
}
