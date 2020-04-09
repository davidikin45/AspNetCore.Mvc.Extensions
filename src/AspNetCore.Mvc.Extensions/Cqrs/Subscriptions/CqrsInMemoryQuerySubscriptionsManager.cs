using AspNetCore.Mvc.Extensions.Cqrs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions
{
    public partial class CqrsInMemoryQuerySubscriptionsManager : ICqrsQuerySubscriptionsManager
    {
        private readonly Dictionary<string, List<QuerySubscriptionInfo>> _handlers;
        private readonly List<Type> _queryTypes;

        public event EventHandler<string> OnQueryRemoved;

        public CqrsInMemoryQuerySubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<QuerySubscriptionInfo>>() { { "*", new List<QuerySubscriptionInfo>() } };
            _queryTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddDynamicSubscription<R, QH>(string queryName)
            where QH : IDynamicQueryHandler<object, R>
        {
            DoAddSubscription(queryName, null, typeof(R), typeof(QH), true);
        }

        public void RemoveDynamicSubscription<R, QH>(string eventName)
        where QH : IDynamicQueryHandler<object, R>
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<object, R, QH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        public void AddSubscription(Type queryType, Type queryHandlerType)
        {
            var queryName = GetQueryKey(queryType);
            var returnType = queryType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQuery<>)).GetGenericArguments()[0];

            DoAddSubscription(queryName, queryType, returnType, queryHandlerType, false);

            if (!_queryTypes.Contains(queryType))
            {
                _queryTypes.Add(queryType);
            }
        }

        public void AddSubscription<Q, R, QH>()
            where Q : IQuery<R>
            where QH : ITypedQueryHandler<Q, R>
        {
            var queryName = GetQueryKey<Q>();

            DoAddSubscription(queryName, typeof(Q), typeof(R), typeof(QH), false);

            if (!_queryTypes.Contains(typeof(Q)))
            {
                _queryTypes.Add(typeof(Q));
            }
        }

        private void DoAddSubscription(string queryName, Type queryType, Type returnType, Type handlerType, bool isDynamic)
        {
            if (!_handlers.ContainsKey(queryName))
            {
                _handlers.Add(queryName, new List<QuerySubscriptionInfo>());
            }

            if (_handlers[queryName].Any())
            {
                throw new ArgumentException($"Handler Type already registered for '{queryName}'");
            }

            if(isDynamic)
            {
                _handlers[queryName].Add(QuerySubscriptionInfo.Dynamic(queryName, returnType, handlerType));
            }
            else
            {
                _handlers[queryName].Add(QuerySubscriptionInfo.Typed(queryName, queryType, returnType, handlerType));
            }
        }

        public void RemoveSubscription<Q, R, QH>()
           where Q : IQuery<R>
           where QH : ITypedQueryHandler<Q, R>
        {
            var handlerToRemove = FindSubscriptionToRemove<Q, R, QH>();
            var queryName = GetQueryKey<Q>();
            DoRemoveHandler(queryName, handlerToRemove);
        }

        private void DoRemoveHandler(string queryName, QuerySubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[queryName].Remove(subsToRemove);
                if (!_handlers[queryName].Any())
                {
                    _handlers.Remove(queryName);
                    var queryType = _queryTypes.SingleOrDefault(e => e.Name == queryName);
                    if (queryType != null)
                    {
                        _queryTypes.Remove(queryType);
                    }
                    RaiseOnQueryRemoved(queryName);
                }

            }
        }

        public IEnumerable<QuerySubscriptionInfo> GetSubscriptionsForQuery<TResult>(IQuery<TResult> query)
        {
            var queryName = GetQueryKey(query.GetType());
            return GetSubscriptionsForQuery(queryName);
        }

        public IEnumerable<QuerySubscriptionInfo> GetSubscriptionsForQuery<Q, TResult>() where Q : IQuery<TResult>
        {
            var queryName = GetQueryKey<Q>();
            return GetSubscriptionsForQuery(queryName);
        }

        public IEnumerable<QuerySubscriptionInfo> GetSubscriptionsForQuery(string queryName) => queryName == "*" ? _handlers["*"] : (_handlers.ContainsKey(queryName) ? _handlers[queryName] : new List<QuerySubscriptionInfo>()).Concat(_handlers["*"]);

        public IEnumerable<string> GetQueries() => _handlers.Keys;

        private void RaiseOnQueryRemoved(string queryName)
        {
            var handler = OnQueryRemoved;
            if (handler != null)
            {
                OnQueryRemoved(this, queryName);
            }
        }

        private QuerySubscriptionInfo FindDynamicSubscriptionToRemove<Q, R, QH>(string eventName)
             where QH : IDynamicQueryHandler<Q, R>
        {
            return DoFindSubscriptionToRemove(eventName, typeof(QH));
        }

        private QuerySubscriptionInfo FindSubscriptionToRemove<Q, R, QH>()
              where Q : IQuery<R>
           where QH : ITypedQueryHandler<Q, R>
        {
            var queryName = GetQueryKey<Q>();
            return DoFindSubscriptionToRemove(queryName, typeof(QH));
        }

        private QuerySubscriptionInfo DoFindSubscriptionToRemove(string queryName, Type handlerType)
        {
            if (!HasSubscriptionsForQuery(queryName))
            {
                return null;
            }

            return _handlers[queryName].SingleOrDefault(s => s.HandlerType == handlerType);

        }

        public bool HasSubscriptionsForQuery<Q, R>() where Q : IQuery<R>
        {
            var key = GetQueryKey<Q>();
            return HasSubscriptionsForQuery(key);
        }

        public bool HasSubscriptionsForQuery(string queryName) => (_handlers.ContainsKey(queryName) && _handlers[queryName].Count > 0) || (_handlers.ContainsKey("*") && _handlers["*"].Count > 0);

        public Type GetQueryTypeByName(string queryName) => _queryTypes.SingleOrDefault(t => t.Name == queryName);

        public string GetQueryKey<Q>()
        {
            return GetQueryKey(typeof(Q));
        }

        private string GetQueryKey(Type queryType)
        {
            return queryType.Name;
        }

        public IReadOnlyDictionary<string, QuerySubscriptionInfo> GetSubscriptions()
        {
            return new ReadOnlyDictionary<string, QuerySubscriptionInfo>(_handlers.Where(kvp => kvp.Value.Count > 0).ToDictionary(k => k.Key, v => v.Value.First()));
        }
    }
}
