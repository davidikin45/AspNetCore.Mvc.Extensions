using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.DomainEvents
{
    public class DomainEventBusInMemory : IDomainEventBus
    {
        public static bool DispatchPostCommitEventsInParellel = true;

        protected readonly IServiceProvider _serviceProvider;
        public IDomainEventBusSubscriptionsManager DomainEventSubscriptionsManager { get; }

        public DomainEventBusInMemory(
            IServiceProvider serviceProvider,
            IDomainEventBusSubscriptionsManager domainEventSubscriptionsManager)
        {
            _serviceProvider = serviceProvider;
            DomainEventSubscriptionsManager = domainEventSubscriptionsManager;
        }

        #region Publish Pre Commit InProcess Domain Events
        public async Task PublishPreCommitAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var eventName = DomainEventSubscriptionsManager.GetEventKey(domainEvent.GetType());

            if(DomainEventSubscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                var handlerTypes = DomainEventSubscriptionsManager.GetHandlersForEvent(domainEvent);

                foreach (var handlerType in handlerTypes)
                {
                    IEnumerable<dynamic> handlerInstances = _serviceProvider.GetServices(handlerType.HandlerType);

                    foreach (var handler in handlerInstances)
                    {
                        Result result = await handler.HandlePreCommitAsync(eventName, (dynamic)domainEvent, cancellationToken).ConfigureAwait(false);
                        if (result.IsFailure)
                        {
                            throw new Exception("Pre Commit Event Failed");
                        }
                    }
                }
            }
        }
        #endregion

        #region Publish Post Commit Integration Events
        public async Task PublishPostCommitBatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            if (DispatchPostCommitEventsInParellel)
            {
                await Task.Run(() => Parallel.ForEach(domainEvents, async domainEvent =>
                {
                    await PublishPostCommitAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                }));
            }
            else
            {
                foreach (var domainEvent in domainEvents)
                {
                    await PublishPostCommitAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task PublishPostCommitAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var eventName = DomainEventSubscriptionsManager.GetEventKey(domainEvent.GetType());
            var payload = JsonConvert.SerializeObject(domainEvent);

            try
            {
                await ProcessPostCommitAsync(eventName, payload).ConfigureAwait(false);
            }
            catch
            {
                //Log InProcess Post commit event failed
            }
        }
        #endregion

        #region Handle Post Commit Events
        public async Task ProcessPostCommitAsync(string eventName, string payload)
        {
            if (DomainEventSubscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = DomainEventSubscriptionsManager.GetHandlersForEvent(eventName).ToList();

                if (DispatchPostCommitEventsInParellel)
                {
                    await Task.Run(() => Parallel.ForEach(subscriptions, async subscription =>
                    {
                        await Task.Run(() => Parallel.For(0, subscription.HandlerCount,  async i =>
                        {
                            await TryProcessPostCommitHandlerAsync(eventName, payload, subscription.HandlerType.FullName, i).ConfigureAwait(false);
                        }));
                    }));
                }
                else
                {
                    foreach (var subscription in subscriptions)
                    {
                        for (int i = 0; i < subscription.HandlerCount; i++)
                        {
                            await TryProcessPostCommitHandlerAsync(eventName, payload, subscription.HandlerType.FullName, i).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        protected virtual async Task TryProcessPostCommitHandlerAsync(string eventName, string payload, string handlerType, int handlerIndex)
        {
            try
            {
                await ProcessPostCommitHandlerAsync(eventName, payload, handlerType, handlerIndex).ConfigureAwait(false);
            }
            catch
            {
                //Log InProcess Post commit event failed
            }
        }

        public async Task ProcessPostCommitHandlerAsync(string eventName, string payload, string handlerType, int handlerIndex)
        {
            if(DomainEventSubscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscription = DomainEventSubscriptionsManager.GetHandlersForEvent(eventName).FirstOrDefault(s => s.HandlerType.FullName == handlerType);
                if (subscription != null)
                {
                    Type handlerTypeDeclaration = subscription.HandlerType;

                    dynamic domainEvent;
                    if (subscription.IsDynamic)
                    {
                        domainEvent = JObject.Parse(payload);
                    }
                    else
                    {
                        var eventType = DomainEventSubscriptionsManager.GetEventTypeByName(eventName);
                        domainEvent = JsonConvert.DeserializeObject(payload, eventType);
                    }

                    await DispatchPostCommitAsync(handlerTypeDeclaration, handlerIndex, eventName, domainEvent).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("Invalid handler type");
                }
            }
            else
            {
                throw new Exception("Invalid event");
            }
        }

        private async Task DispatchPostCommitAsync(Type handlerType, int handlerIndex, string eventName, dynamic domainEvent)
        {
            //Each Post Commit Domain Event is a Unit of Work which could trigger many commands.
            using (var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.BeginUnitOfWork();

                IEnumerable<dynamic> handlers = scope.ServiceProvider.GetServices(handlerType);

                dynamic handler = handlers.Skip(handlerIndex).Take(1).FirstOrDefault();

                if(handler == null)
                {
                    throw new Exception("Invalid handler index");
                }

                Result result = await handler.HandlePostCommitAsync(eventName, domainEvent, default(CancellationToken)).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    throw new Exception("Post Commit Event Failed");
                }

                await scope.ServiceProvider.CompleteUnitOfWorkAsync();
            }
        }
        #endregion
    }
}
