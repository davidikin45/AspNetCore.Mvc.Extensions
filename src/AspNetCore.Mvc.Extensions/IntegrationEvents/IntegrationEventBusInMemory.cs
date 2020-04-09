using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents
{
    //Each microservice has a seperate queue 
    //Publisher > Exchange > Queue > Consumer
    public class IntegrationEventBusInMemory : IIntegrationEventBus
    {
        protected readonly ILogger<IntegrationEventBusInMemory> _logger;
        protected readonly IIntegrationEventBusSubscriptionsManager _subsManager;
        protected readonly IServiceProvider _serviceProvider;
        public IntegrationEventBusInMemory(ILogger<IntegrationEventBusInMemory> logger,
            IServiceProvider serviceProvider, IIntegrationEventBusSubscriptionsManager subsManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new IntegrationEventBusInMemorySubscriptionsManager();

            _serviceProvider = serviceProvider;
        }

        public virtual async Task PublishAsync(IntegrationEvent integrationEvent)
        {
            var eventName = _subsManager.GetEventKey(integrationEvent.GetType());
            var payload = JsonConvert.SerializeObject(integrationEvent);

            await ProcessEventAsync(eventName, payload).ConfigureAwait(false);
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>
        {
            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            _subsManager.AddSubscription<T, TH>();
        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _subsManager.Clear();
        }

        public async Task ProcessEventAsync(string eventName, string payload)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                //Each Integration Event is a Unit of Work which could trigger many commands.
                using (var scope = _serviceProvider.CreateScope())
                {
                    scope.ServiceProvider.BeginUnitOfWork();

                    foreach (var subscription in subscriptions)
                    {
                        for (int i = 0; i < subscription.HandlerCount; i++)
                        {
                            await ProcessEventHandlerAsync(eventName, payload, subscription.HandlerType.FullName, i, scope.ServiceProvider).ConfigureAwait(false);
                        }
                    }

                    await scope.ServiceProvider.CompleteUnitOfWorkAsync();
                }
            }
        }

        public async Task ProcessEventHandlerAsync(string eventName, string payload, string handlerType, int handlerIndex, IServiceProvider serviceProvider)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscription = _subsManager.GetHandlersForEvent(eventName).FirstOrDefault(s => s.HandlerType.FullName == handlerType);
                if (subscription != null)
                {

                    dynamic integrationEvent;
                    if (subscription.IsDynamic)
                    {
                        integrationEvent = JObject.Parse(payload);
                    }
                    else
                    {
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        integrationEvent = JsonConvert.DeserializeObject(payload, eventType);
                    }

                    await DispatchEventAsync(subscription.HandlerType, handlerIndex, eventName, integrationEvent, serviceProvider).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("Invalid handler type");
                }
            }
        }

        private async Task DispatchEventAsync(Type handlerType, int handlerIndex, string eventName, dynamic integrationEvent, IServiceProvider serviceProvider)
        {
            IEnumerable<dynamic> handlers = serviceProvider.GetServices(handlerType);

            dynamic handler = handlers.Skip(handlerIndex).Take(1).First();

            if (handler == null)
            {
                throw new Exception("Invalid handler index");
            }

            Result result = await handler.HandleAsync(eventName, integrationEvent, default(CancellationToken)).ConfigureAwait(false);
            if (result.IsFailure)
            {
                throw new Exception("Integration Event Failed");
            }
            //var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            //await (Task)concreteType.GetMethod("HandleAsync").Invoke(handler, new object[] { integrationEvent, default(CancellationToken) });

        }
    }
}
