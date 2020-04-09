using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public abstract class ScopedHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        public ScopedHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected abstract Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken);
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                await ExecuteAsync(scope.ServiceProvider, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class ScopedHostedService<TScopedProcessingService> : ScopedHostedService where TScopedProcessingService : IScopedProcessingService
    {
        public object[] Arguments { get; set; }
        public ScopedHostedService(IServiceProvider serviceProvider)
            :base(serviceProvider)
        {
           
        }

        protected override Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken)
        {
            var workItem = ActivatorUtilities.CreateInstance<TScopedProcessingService>(scopedServiceProvider, Arguments);
            return workItem.ExecuteAsync(stoppingToken);
        }
    }
}
