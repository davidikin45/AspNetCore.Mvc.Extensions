using AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs
{
    //No service scopes required.
    //When triggered from a Controller the ASP.NET Core Request Scope will be used. Each Dispatch = Unit of Work.
    //when triggered from a Pre Commit Domain Event Handler the Parent Scope (Request, Domain Event Mediator or Integration Event Mediator) will be used. Parent Unit of Work = Unit of Work.
    //when triggered from a Post Commit Domain Event Handler the Domain Event Mediator Scope will be used. Domain Event Handle = Unit of Work.
    //When triggered from an Integration Event Handler the Integration Event Mediator Scope will be used. Integration Event = Unit of Work.

    public sealed class CqrsMediator : ICqrsMediator
    {
        private readonly IServiceProvider _provider;

        public ICqrsCommandSubscriptionsManager CqrsCommandSubscriptionManager { get; }
        public ICqrsQuerySubscriptionsManager CqrsQuerySubscriptionManager { get; }

        public CqrsMediator(IServiceProvider provider, ICqrsCommandSubscriptionsManager cqrsCommandSubscriptionManager, ICqrsQuerySubscriptionsManager cqrsQuerySubscriptionManager)
        {
            _provider = provider;
            CqrsCommandSubscriptionManager = cqrsCommandSubscriptionManager;
            CqrsQuerySubscriptionManager = cqrsQuerySubscriptionManager;
        }

        public async Task<Result<dynamic>> DispatchCommandAsync(string commandName, string payload, CancellationToken cancellationToken = default)
        {  
            var subscription = CqrsCommandSubscriptionManager.GetSubscriptionsForCommand(commandName).First();
            Type handlerType = subscription.HandlerType;

            //dynamic handler = _provider.GetService(handlerType);
            dynamic handler = subscription.CreateHandler(_provider);

            dynamic typedPayload = null;
            if (subscription.IsDynamic)
            {
                typedPayload = JObject.Parse(payload);
            }
            else
            {
                typedPayload = JsonConvert.DeserializeObject(payload, subscription.CommandType);
            }

            Result<dynamic> result = await handler.HandleAsync(commandName, typedPayload, cancellationToken);
            return result;
        }

        public async Task<Result<R>> DispatchAsync<R>(ICommand<R> command, CancellationToken cancellationToken = default)
        {
            var subscription = CqrsCommandSubscriptionManager.GetSubscriptionsForCommand(command).First();
            Type handlerType = subscription.HandlerType;//Interface OR Concrete. 
            var commandName = subscription.CommandName;

            //dynamic handler = _provider.GetService(handlerType);
            dynamic handler = subscription.CreateHandler(_provider);

            Result<R> result = await handler.HandleAsync(commandName, (dynamic)command, cancellationToken);

            return result;
        }

        public async Task<Result<dynamic>> DispatchQueryAsync(string queryName, string payload, CancellationToken cancellationToken = default)
        {
            var subscription = CqrsQuerySubscriptionManager.GetSubscriptionsForQuery(queryName).First();
            Type handlerType = subscription.HandlerType;

            //dynamic handler =  _provider.GetService(handlerType);
            dynamic handler = subscription.CreateHandler(_provider);

            dynamic typedPayload = null;
            if (subscription.IsDynamic)
            {
                typedPayload = JObject.Parse(payload);
            }
            else
            {
                typedPayload = JsonConvert.DeserializeObject(payload, subscription.QueryType);
            }

            Result<dynamic> result = await handler.HandleAsync(queryName, typedPayload, cancellationToken);
            return result;
        }

        public async Task<Result<R>> DispatchAsync<R>(IQuery<R> query, CancellationToken cancellationToken = default)
        {
            var subscription = CqrsQuerySubscriptionManager.GetSubscriptionsForQuery(query).First();
            Type handlerType = subscription.HandlerType;
            var queryName = subscription.QueryName;

            //dynamic handler = _provider.GetService(handlerType);
            dynamic handler = subscription.CreateHandler(_provider);

            Result<R> result = await handler.HandleAsync(queryName, (dynamic)query, cancellationToken);

            return result;
        }
    }
}
