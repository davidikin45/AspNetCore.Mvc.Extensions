using AspNetCore.Mvc.Extensions.Data.Initializers;
using Hangfire.Initialization;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Hangfire
{
    public class HangfireInitializerDropCreate : IDbInitializer
    {
        public async Task InitializeAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await InitializeSchemaAsync(connectionString, cancellationToken);
            await InitializeDataAsync(connectionString, null, cancellationToken);
        }

        public async Task InitializeSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await HangfireInitializer.EnsureTablesDeletedAsync(connectionString, cancellationToken);
            await HangfireInitializer.EnsureDbAndTablesCreatedAsync(connectionString, null, cancellationToken);
        }

        public Task InitializeDataAsync(string connectionString, string tenantId, CancellationToken cancellationToken = default)
        {
            Seed(connectionString, tenantId);

            return OnSeedCompleteAsync(connectionString);
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
