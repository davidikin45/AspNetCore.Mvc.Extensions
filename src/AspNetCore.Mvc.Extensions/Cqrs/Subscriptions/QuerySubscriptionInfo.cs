using AspNetCore.Mvc.Extensions.Cqrs;
using System;

namespace AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions
{
    public partial class CqrsInMemoryQuerySubscriptionsManager : ICqrsQuerySubscriptionsManager
    {
        public class QuerySubscriptionInfo
        {
            public bool IsDynamic { get; }
            public string QueryName { get; }
            public Type QueryType { get; }
            public Type ReturnType { get; }
            public Type HandlerType { get; }

            private readonly Func<IServiceProvider, object> _factory;

            private QuerySubscriptionInfo(bool isDynamic, string queryName, Type queryType, Type returnType, Type handlerType)
            {
                IsDynamic = isDynamic;
                QueryName = queryName;
                QueryType = queryType;
                ReturnType = returnType;
                HandlerType = handlerType;

                _factory = CqrsServiceCollectionExtensions.CreateFactory(queryType, handlerType, true);
            }

            public static QuerySubscriptionInfo Typed(string queryName, Type queryType, Type returnType, Type handlerType)
            {
                return new QuerySubscriptionInfo(false, queryName, queryType, returnType, handlerType);
            }

            public static QuerySubscriptionInfo Dynamic(string queryName, Type returnType, Type handlerType)
            {
                return new QuerySubscriptionInfo(true, queryName, null, returnType, handlerType);
            }
            public object CreateHandler(IServiceProvider serviceProvider) => _factory(serviceProvider);
        }
    }
}
