using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Initializers
{
    public interface IDbContextInitializer<TDbContext>
        where TDbContext : DbContext
    {
        Task InitializeAsync(TDbContext context, CancellationToken cancellationToken);
        Task InitializeSchemaAsync(TDbContext context, CancellationToken cancellationToken);
        Task InitializeDataAsync(TDbContext context, string tenantId, CancellationToken cancellationToken);

        void Seed(TDbContext context, string tenantId);
    }
}
