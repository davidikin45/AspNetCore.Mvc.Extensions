using System;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions
{
    public partial class IntegrationEventBusInMemorySubscriptionsManager : IIntegrationEventBusSubscriptionsManager
    {
        public class SubscriptionInfo
        {
            public bool IsDynamic { get; }
            public Type HandlerType { get; }

            public int HandlerCount { get; private set; } = 1;

            public void IncrementHandlerCount()
            {
                HandlerCount++;
            }

            private SubscriptionInfo(bool isDynamic, Type handlerType)
            {
                IsDynamic = isDynamic;
                HandlerType = handlerType;
            }

            public static SubscriptionInfo Dynamic(Type handlerType)
            {
                return new SubscriptionInfo(true, handlerType);
            }
            public static SubscriptionInfo Typed(Type handlerType)
            {
                return new SubscriptionInfo(false, handlerType);
            }
        }
    }
}
