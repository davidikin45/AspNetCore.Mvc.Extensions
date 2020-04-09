using System;
using System.Collections.Generic;
using static AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions.IntegrationEventBusInMemorySubscriptionsManager;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions
{
    public interface IIntegrationEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;

        void AddSubscription<T, TH>()
           where T : IntegrationEvent
           where TH : IIntegrationEventHandler<T>;

        void RemoveSubscription<T, TH>()
             where TH : IIntegrationEventHandler<T>
             where T : IntegrationEvent;

        void AddDynamicSubscription<TH>(string eventType)
         where TH : IDynamicIntegrationEventHandler<object>;

        void RemoveDynamicSubscription<TH>(string eventType)
            where TH : IDynamicIntegrationEventHandler<object>;

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
        bool HasSubscriptionsForEvent(string eventType);
        Type GetEventTypeByName(string eventType);
        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(IntegrationEvent @event);
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        string GetEventKey<T>();
        string GetEventKey(Type integrationEventType);
    }
}
