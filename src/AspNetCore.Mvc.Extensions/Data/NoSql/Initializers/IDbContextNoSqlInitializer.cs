using EntityFrameworkCore.Initialization.NoSql;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.NoSql.Initializers
{
    public interface IDbContextNoSqlInitializer<TDbContext>
        where TDbContext : DbContextNoSql
    {
        Task InitializeAsync(TDbContext context, CancellationToken cancellationToken);
        Task InitializeSchemaAsync(TDbContext context, CancellationToken cancellationToken);
    }
}
