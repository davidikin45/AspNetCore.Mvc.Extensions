using AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents
{
    //Each microservice has a seperate queue 
    //Publisher > Exchange > Queue > Consumer
    public class IntegrationEventBusHangFire : IntegrationEventBusInMemory
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IntegrationEventBusHangFireOptions _options;

        public IntegrationEventBusHangFire(IBackgroundJobClient backgroundJobClient, ILogger<IntegrationEventBusHangFire> logger,
            IServiceProvider serviceProvider, IIntegrationEventBusSubscriptionsManager subsManager, IOptions<IntegrationEventBusHangFireOptions> options)
            :base(logger, serviceProvider, subsManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _options = options.Value;
        }

        public override Task PublishAsync(IntegrationEvent integrationEvent)
        {
            var eventName = _subsManager.GetEventKey(integrationEvent.GetType());
            var payload = JsonConvert.SerializeObject(integrationEvent);

            if(_options.ServerNames != null)
            {
                foreach (var serverName in _options.ServerNames)
                {
                    var job = Job.FromExpression<IIntegrationEventBus>(m => m.ProcessEventAsync(eventName, payload));
                    var queue = new EnqueuedState(serverName);
                    _backgroundJobClient.Create(job, queue);
                }
            }

            return Task.CompletedTask;
        }
    }

    public class IntegrationEventBusHangFireOptions
    {
        public string[] ServerNames { get; set; }
    }
}
