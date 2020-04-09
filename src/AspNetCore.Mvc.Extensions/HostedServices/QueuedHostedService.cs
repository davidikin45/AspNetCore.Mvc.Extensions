using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    //https://stackoverflow.com/questions/51349826/parallel-queued-background-tasks-with-hosted-services-in-asp-net-core
    public class QueuedHostedService : BackgroundService
    {
        private IServiceProvider _serviceProvder;
        private CancellationTokenSource _shutdown =
        new CancellationTokenSource();
        private List<Task> _backgroundTasks = new List<Task>();
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly QueuedHostedServiceOptions _options;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue,
            IServiceProvider serviceProvder,
            ILogger<QueuedHostedService> logger,
            IOptions<QueuedHostedServiceOptions> options)
        {
            TaskQueue = taskQueue;
            _serviceProvder = serviceProvder;
            _logger = logger;
            _options = options.Value;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Queued Hosted Service is running. Worker Count:{_options.WorkerCount}");

            for (int i = 0; i < _options.WorkerCount; i++)
            {
                _backgroundTasks.Add(Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await BackgroundProcessing();
                    }
                }, stoppingToken));
            }

            await Task.WhenAll(_backgroundTasks);
        }

        private async Task BackgroundProcessing()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                var workItem =
                    await TaskQueue.DequeueAsync(_shutdown.Token);

                try
                {
                    using(var scope = _serviceProvder.CreateScope())
                    {
                        await workItem(scope.ServiceProvider, _shutdown.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            _shutdown.Cancel();

            return Task.WhenAny(Task.WhenAll(_backgroundTasks),
                    Task.Delay(Timeout.Infinite, stoppingToken));
        }
    }

    public class QueuedHostedServiceOptions
    {
        public int WorkerCount { get; set; } = 1; // Math.Min(Environment.ProcessorCount * 5, 20);
    }
}
