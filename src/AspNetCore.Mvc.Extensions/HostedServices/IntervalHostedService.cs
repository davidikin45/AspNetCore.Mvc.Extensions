using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public class IntervalHostedService<TService> : BackgroundService
     where TService : IScopedProcessingService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public TimeSpan Interval { get; private set; }

        public IntervalHostedService(IServiceProvider services, ILogger<IntervalHostedService<TService>> logger, TimeSpan interval)
        {
            Interval = interval;
            _services = services;
            _logger = logger;
        }

        private async Task ScheduledTask(CancellationToken ct)
        {
            do
            {
                using (var scope = _services.CreateScope())
                {
                    var scopedProcessingService = scope.ServiceProvider.GetRequiredService<TService>();

                    _logger.LogInformation("Executing IntervalJob {IntervalJob}", typeof(TService));
                    try
                    {
                        await scopedProcessingService.ExecuteAsync(ct);
                        _logger.LogInformation("CronJob {IntervalJob} completed successfully", typeof(TService));
                    }
                    catch
                    {
                        _logger.LogInformation("CronJob {IntervalJob} failed", typeof(TService));
                    }
                }

                var currentTime = DateTime.UtcNow;
                var nextOccurence = currentTime.AddSeconds(Interval.TotalSeconds);

                var delay = nextOccurence - currentTime;
                if (delay.Seconds > 0)
                {
                    await Task.Delay(delay, ct);
                }
            }
            while (!ct.IsCancellationRequested);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return ScheduledTask(stoppingToken);
        }
    }
}
