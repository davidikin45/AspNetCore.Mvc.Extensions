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
        }

        public async Task InitializeSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await HangfireInitializer.EnsureTablesDeletedAsync(connectionString, cancellationToken);
            await HangfireInitializer.EnsureDbAndTablesCreatedAsync(connectionString, null, cancellationToken);
        }
    }
}
