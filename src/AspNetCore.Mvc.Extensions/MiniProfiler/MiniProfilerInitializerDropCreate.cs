using AspNetCore.Mvc.Extensions.Data.Initializers;
using MiniProfiler.Initialization;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.MiniProfiler
{
    public class MiniProfilerInitializerDropCreate : IDbInitializer
    {
        public async Task InitializeAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await InitializeSchemaAsync(connectionString, cancellationToken);
            await InitializeDataAsync(connectionString, null, cancellationToken);
        }

        public async Task InitializeSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await MiniProfilerInitializer.EnsureTablesDeletedAsync(connectionString, cancellationToken);
            await MiniProfilerInitializer.EnsureDbAndTablesCreatedAsync(connectionString, cancellationToken);           
        }

        public async Task InitializeDataAsync(string connectionString, string tenantId, CancellationToken cancellationToken = default)
        {
            Seed(connectionString, tenantId);

            await OnSeedCompleteAsync(connectionString);
        }

        public virtual void Seed(string connectionString, string tenantId)
        {

        }

        public virtual Task OnSeedCompleteAsync(string connectionString)
        {
            return Task.CompletedTask;
        }
    }
}
