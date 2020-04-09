using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions
{
    public partial class IntegrationEventBusInMemorySubscriptionsManager : IIntegrationEventBusSubscriptionsManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public IntegrationEventBusInMemorySubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>() { { "*", new List<SubscriptionInfo>() } };
            _eventTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>
        {
            DoAddSubscription(eventName, typeof(TH), isDynamic: true);
        }

        public void AddSubscription(Type eventType, Type eventHandlerType)
        {
            var eventName = GetEventKey(eventType);

            DoAddSubscription(eventName, eventHandlerType, isDynamic: false);

            if (!_eventTypes.Contains(eventType))
            {
                _eventTypes.Add(eventType);
            }
        }

        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            DoAddSubscription(eventName, typeof(TH), isDynamic: false);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        private void DoAddSubscription(string eventName, Type handlerType, bool isDynamic)
        {
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                var susbcription = _handlers[eventName].First(s => s.HandlerType == handlerType);
                susbcription.IncrementHandlerCount();

                //throw new ArgumentException(
                //    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }
            else
            {
                if (isDynamic)
                {
                    _handlers[eventName].Add(SubscriptionInfo.Dynamic(handlerType));
                }
                else
                {
                    _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
                }
            }
        }


        public void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<object, TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }


        public void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }

        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(IntegrationEvent integrationEvent)
        {
            var key = GetEventKey(integrationEvent.GetType());
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => eventName == "*" ? _handlers["*"] : (_handlers.ContainsKey(eventName) ? _handlers[eventName] : new List<SubscriptionInfo>()).Concat(_handlers["*"]);

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            if (handler != null)
            {
                OnEventRemoved(this, eventName);
            }
        }

        private SubscriptionInfo FindDynamicSubscriptionToRemove<TIntegrationEvent,TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<TIntegrationEvent>
        {
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }


        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
             where T : IntegrationEvent
             where TH : IIntegrationEventHandler<T>
        {
            var type = GetEventKey<T>();
            return DoFindSubscriptionToRemove(type, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var eventName = GetEventKey<T>();
            return HasSubscriptionsForEvent(eventName);
        }
        public bool HasSubscriptionsForEvent(string eventName) => (_handlers.ContainsKey(eventName) && _handlers[eventName].Count > 0) || (_handlers.ContainsKey("*") && _handlers["*"].Count > 0);

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName) ?? _eventTypes.SingleOrDefault(t => t.Name == "*");

        public string GetEventKey<T>()
        {
            return GetEventKey(typeof(T));
        }

        public string GetEventKey(Type integrationEventType)
        {
            if (integrationEventType == typeof(IntegrationEvent))
                return "*";
            else
                return integrationEventType.Name;
        }
    }
}
