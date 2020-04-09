using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public abstract class LoopHostedService : BackgroundService
    {
        private readonly int _refreshIntervalInSeconds;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public LoopHostedService(IServiceProvider serviceProvider, ILogger logger, int refreshIntervalInSeconds)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _refreshIntervalInSeconds = refreshIntervalInSeconds;

            if(WindowsServiceHelpers.IsWindowsService())
            {
                _logger.LogInformation("Running as a windows service!");
            }
        }


        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Ending Interval processing.");
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWorkAsync(_serviceProvider, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(_refreshIntervalInSeconds), stoppingToken);
            }
        }

        public abstract Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken stoppingToken);

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            await base.StopAsync(cancellationToken);

            _logger.LogInformation("Completed shutdown in {Milliseconds}ms.", sw.ElapsedMilliseconds);

        }
    }
}
