using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public class DbInitializeHostedService<TDbContext> : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<IServiceProvider, TDbContext> _configure;

        public DbInitializeHostedService(IServiceProvider serviceProvider, Action<IServiceProvider, TDbContext> configure)
        {
            _serviceProvider = serviceProvider;
            _configure = configure;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
                _configure(scope.ServiceProvider, db);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
