using AspNetCore.Mvc.Extensions.Cqrs;
using System;
using System.Collections.Generic;
using static AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions.CqrsInMemoryQuerySubscriptionsManager;

namespace AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions
{
    public interface ICqrsQuerySubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnQueryRemoved;

        void AddDynamicSubscription<R, QH>(string queryName)
        where QH : IDynamicQueryHandler<object, R>;

        void RemoveDynamicSubscription<R, QH>(string queryName)
        where QH : IDynamicQueryHandler<object, R>;

        void AddSubscription<Q, R, QH>()
           where Q : IQuery<R>
           where QH : ITypedQueryHandler<Q, R>;

        void RemoveSubscription<Q, R, QH>()
              where Q : IQuery<R>
             where QH : ITypedQueryHandler<Q, R>;

        bool HasSubscriptionsForQuery<Q, TResult>() where Q : IQuery<TResult>;
        bool HasSubscriptionsForQuery(string queryName);
        Type GetQueryTypeByName(string queryName);

        void Clear();

        IReadOnlyDictionary<string, QuerySubscriptionInfo> GetSubscriptions();
        IEnumerable<QuerySubscriptionInfo> GetSubscriptionsForQuery<TResult>(IQuery<TResult> query);
        IEnumerable<QuerySubscriptionInfo> GetSubscriptionsForQuery<Q, TResult>() where Q : IQuery<TResult>;
        IEnumerable<QuerySubscriptionInfo> GetSubscriptionsForQuery(string queryName);
        IEnumerable<string> GetQueries();
        string GetQueryKey<T>();
    }
}
