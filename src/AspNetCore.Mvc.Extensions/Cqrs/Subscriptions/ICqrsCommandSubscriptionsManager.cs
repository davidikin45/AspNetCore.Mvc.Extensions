using AspNetCore.Mvc.Extensions.Cqrs;
using System;
using System.Collections.Generic;
using static AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions.CqrsInMemoryCommandSubscriptionsManager;

namespace AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions
{
    public interface ICqrsCommandSubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnCommandRemoved;

        void AddDynamicSubscription<R, CH>(string commandName)
            where CH: IDynamicCommandHandler<object, R>;

        void AddDynamicSubscription<CH>(string commandName)
         where CH : IDynamicCommandHandler<object>;

        void RemoveDynamicSubscription<R, CH>(string eventName)
         where CH : IDynamicCommandHandler<object, R>;

        void AddSubscription(Type commandType, Type commandHandlerType);

        void AddSubscription<C, R, CH>()
          where C : ICommand<R>
          where CH : ITypedCommandHandler<C, R>;

        void AddSubscription<C, CH>()
          where C : ICommand
          where CH : ITypedCommandHandler<C>;

        void RemoveSubscription<C, R, CH>()
             where C : ICommand<R>
             where CH : ITypedCommandHandler<C, R>;

        bool HasSubscriptionsForCommand<C, R>() where C : ICommand<R>;
        bool HasSubscriptionsForCommand(string commandName);
        Type GetCommandTypeByName(string commandName);
        void Clear();

        IReadOnlyDictionary<string, CommandSubscriptionInfo> GetSubscriptions();
        IEnumerable<CommandSubscriptionInfo> GetSubscriptionsForCommand<R>(ICommand<R> command);
        IEnumerable<CommandSubscriptionInfo> GetSubscriptionsForCommand<C, R>() where C : ICommand<R>;
        IEnumerable<CommandSubscriptionInfo> GetSubscriptionsForCommand(string commandName);
        IEnumerable<string> GetCommands();

        string GetCommandKey<T>();
    }
}
