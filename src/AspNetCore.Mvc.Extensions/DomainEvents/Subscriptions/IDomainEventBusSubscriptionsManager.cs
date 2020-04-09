using System;
using System.Collections.Generic;
using static AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions.DomainEventBusInMemorySubscriptionsManager;

namespace AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions
{
    public interface IDomainEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;

        void AddSubscription<T, TH>()
           where T : DomainEvent
           where TH : IDomainEventHandler<T>;

        void RemoveSubscription<T, TH>()
             where TH : IDomainEventHandler<T>
             where T : DomainEvent;

        void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicDomainEventHandler<object>;

        void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicDomainEventHandler<object>;

        bool HasSubscriptionsForEvent<T>() where T : DomainEvent;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(DomainEvent domainEvent);
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : DomainEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        string GetEventKey(Type domainEventType);
        string GetEventKey<T>();
    }
}
