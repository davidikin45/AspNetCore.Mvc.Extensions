using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public class CronHostedService<TService> : BackgroundService
      where TService : IScopedProcessingService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        public object[] Arguments { get; set; }

        public string[] CronSchedules { get; private set; }

        public CronHostedService(IServiceProvider services, ILogger<CronHostedService<TService>> logger, params string[] cronSchedules)
        {
            CronSchedules = cronSchedules;
            _services = services;
            _logger = logger;
        }

        public CronHostedService(IServiceProvider services, ILogger<CronHostedService<TService>> logger)
        {
            _services = services;
            _logger = logger;
        }

        private async Task ScheduledTask(CancellationToken ct)
        {
            if (CronSchedules == null || CronSchedules.Length == 0)
            {
                var cronSchedule = (CronJobAttribute)typeof(TService).GetCustomAttributes(typeof(CronJobAttribute), true).FirstOrDefault();
                if (cronSchedule != null)
                {
                    CronSchedules = cronSchedule.Schedules;
                }
            }

            if (CronSchedules == null || CronSchedules.Length == 0)
            {
                throw new Exception("Job must pass cron schedules or have a CronJobAttribute");
            }

            var schedules = CronSchedules.Select(schedule => CrontabSchedule.Parse(schedule));

            do
            {
                var currentTime = DateTime.UtcNow;
               var nextOccurence = schedules.Select(schedule => schedule.GetNextOccurrence(currentTime)).Min();

                var delay = nextOccurence - currentTime;
                if(delay.Seconds > 0)
                {
                    await Task.Delay(delay);
                }

                //Unit of Work
                using (var scope = _services.CreateScope())
                {
                    var scopedProcessingService = ActivatorUtilities.CreateInstance<TService>(scope.ServiceProvider, Arguments);
                        //scope.ServiceProvider.GetRequiredService<TService>();

                    _logger.LogInformation("Executing CronJob {CronJob}", typeof(TService));
                    try
                    {
                        await scopedProcessingService.ExecuteAsync(ct);
                        _logger.LogInformation("CronJob {CronJob} completed successfully", typeof(TService));
                    }
                    catch
                    {
                        _logger.LogInformation("CronJob {CronJob} failed", typeof(TService));
                    }
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
