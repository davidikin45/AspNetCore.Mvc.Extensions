using AspNetCore.Mvc.Extensions.Data.Initializers;
using Hangfire.Initialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Hangfire
{
    public class HangfireInitializerCreate : IDbInitializer
    {
        public int Order => 0;

        public async Task InitializeAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await InitializeSchemaAsync(connectionString, cancellationToken);
        }

        public Task InitializeSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            return HangfireInitializer.EnsureDbAndTablesCreatedAsync(connectionString, null,cancellationToken);
        }

        public virtual Task OnSeedCompleteAsync(string connectionString)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
