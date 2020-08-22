using EntityFrameworkCore.Initialization.NoSql;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.NoSql.Initializers
{
    public class ContextInitializerNoSqlDropCreate<TDbContext> : IDbContextNoSqlInitializer<TDbContext>
        where TDbContext : DbContextNoSql
    {
        public async Task InitializeAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeSchemaAsync(context, cancellationToken);
        }

        public Task InitializeSchemaAsync(TDbContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            context.EnsureCollectionsDeleted();
            return Task.CompletedTask;
        }
    }
}
