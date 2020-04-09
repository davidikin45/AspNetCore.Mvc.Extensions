using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HealthChecks
{
    public class HealthCheckGenericPublisher : IHealthCheckPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HealthCheckGenericPublisherOptions _options;

        public HealthCheckGenericPublisher(IServiceProvider serviceProvider, IOptions<HealthCheckGenericPublisherOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            var message = JsonConvert.SerializeObject(report, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return _options.PublishAsync(_serviceProvider, message);
        }
    }

    public class HealthCheckGenericPublisherOptions
    {
        public Func<IServiceProvider, string, Task> PublishAsync { get; set; } = (_, __) => Task.CompletedTask;
    }
}
