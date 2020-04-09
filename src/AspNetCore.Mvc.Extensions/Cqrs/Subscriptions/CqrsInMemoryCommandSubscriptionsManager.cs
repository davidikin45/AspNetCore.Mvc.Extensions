using AspNetCore.Mvc.Extensions.Cqrs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions
{
    public partial class CqrsInMemoryCommandSubscriptionsManager : ICqrsCommandSubscriptionsManager
    {
        private readonly Dictionary<string, List<CommandSubscriptionInfo>> _handlers;
        private readonly List<Type> _commandTypes;

        public event EventHandler<string> OnCommandRemoved;

        public CqrsInMemoryCommandSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<CommandSubscriptionInfo>>() { { "*", new List<CommandSubscriptionInfo>() } };
            _commandTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddDynamicSubscription<R, CH>(string commandName)
           where CH : IDynamicCommandHandler<object,R>
        {
            DoAddSubscription(commandName, null, typeof(R), typeof(CH), true);
        }

        public void AddDynamicSubscription<CH>(string commandName)
           where CH : IDynamicCommandHandler<object>
        {
            DoAddSubscription(commandName, null, null, typeof(CH), true);
        }

        public void RemoveDynamicSubscription<R, CH>(string eventName)
        where CH : IDynamicCommandHandler<object,R>
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<object, R, CH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        public void AddSubscription(Type commandType, Type commandHandlerType)
        {
            var commandName = GetCommandKey(commandType);

            var returnTypeInterface = commandType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>));
            var returnType = returnTypeInterface.GenericTypeArguments[0];
            DoAddSubscription(commandName, commandType, returnType, commandHandlerType, false);

            if (!_commandTypes.Contains(commandType))
            {
                _commandTypes.Add(commandType);
            }
        }

        public void AddSubscription<C, R, CH>()
           where C : ICommand<R>
           where CH : ITypedCommandHandler<C,R>
        {
            var commandName = GetCommandKey<C>();

            DoAddSubscription(commandName, typeof(C), typeof(R), typeof(CH), false);

            if (!_commandTypes.Contains(typeof(C)))
            {
                _commandTypes.Add(typeof(C));
            }
        }

        public void AddSubscription<C, CH>()
            where C : ICommand
            where CH : ITypedCommandHandler<C>
        {
            var commandName = GetCommandKey<C>();

            DoAddSubscription(commandName, typeof(C), null, typeof(CH), false);

            if (!_commandTypes.Contains(typeof(C)))
            {
                _commandTypes.Add(typeof(C));
            }
        }

        private void DoAddSubscription(string commandName, Type commandType, Type returnType, Type handlerType, bool isDynamic)
        {
            if (!_handlers.ContainsKey(commandName))
            {
                _handlers.Add(commandName, new List<CommandSubscriptionInfo>());
            }

            if (_handlers[commandName].Any())
            {
                throw new ArgumentException($"Handler already registered for '{commandName}'");
            }

            if(isDynamic)
            {
                _handlers[commandName].Add(CommandSubscriptionInfo.Dynamic(commandName, returnType, handlerType));
            }
            else
            {
                _handlers[commandName].Add(CommandSubscriptionInfo.Typed(commandName, commandType, returnType, handlerType));
            }
        }

        public void RemoveSubscription<C, R, CH>()
           where C : ICommand<R>
           where CH : ITypedCommandHandler<C, R>
        {
            var handlerToRemove = FindSubscriptionToRemove<C, R, CH>();
            var commandName = GetCommandKey<C>();
            DoRemoveHandler(commandName, handlerToRemove);
        }

        private void DoRemoveHandler(string commandName, CommandSubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[commandName].Remove(subsToRemove);
                if (!_handlers[commandName].Any())
                {
                    _handlers.Remove(commandName);
                    var commandType = _commandTypes.SingleOrDefault(e => e.Name == commandName);
                    if (commandType != null)
                    {
                        _commandTypes.Remove(commandType);
                    }
                    RaiseOnCommandRemoved(commandName);
                }

            }
        }
        public IReadOnlyDictionary<string, CommandSubscriptionInfo> GetSubscriptions()
        {
            return new ReadOnlyDictionary<string, CommandSubscriptionInfo>(_handlers.Where(kvp => kvp.Value.Count > 0).ToDictionary(k => k.Key, v => v.Value.First()));
        }

        public IEnumerable<CommandSubscriptionInfo> GetSubscriptionsForCommand<R>(ICommand<R> command)
        {
            var key = GetCommandKey(command.GetType());
            return GetSubscriptionsForCommand(key);
        }

        public IEnumerable<CommandSubscriptionInfo> GetSubscriptionsForCommand<C, R>() where C : ICommand<R>
        {
            var key = GetCommandKey<C>();
            return GetSubscriptionsForCommand(key);
        }
        public IEnumerable<CommandSubscriptionInfo> GetSubscriptionsForCommand(string commandName) => commandName == "*" ? _handlers["*"] : (_handlers.ContainsKey(commandName) ? _handlers[commandName] : new List<CommandSubscriptionInfo>()).Concat(_handlers["*"]);

        public IEnumerable<string> GetCommands() => _handlers.Keys;

        private void RaiseOnCommandRemoved(string commandName)
        {
            var handler = OnCommandRemoved;
            if (handler != null)
            {
                OnCommandRemoved(this, commandName);
            }
        }

        private CommandSubscriptionInfo FindDynamicSubscriptionToRemove<C, R, CH>(string commandName)
           where CH : IDynamicCommandHandler<C, R>
        {
            return DoFindSubscriptionToRemove(commandName, typeof(CH));
        }

        private CommandSubscriptionInfo FindSubscriptionToRemove<C, R, CH>()
            where C : ICommand<R>
           where CH : ITypedCommandHandler<C, R>
        {
            var commandName = GetCommandKey<C>();
            return DoFindSubscriptionToRemove(commandName, typeof(CH));
        }

        private CommandSubscriptionInfo DoFindSubscriptionToRemove(string commandName, Type handlerType)
        {
            if (!HasSubscriptionsForCommand(commandName))
            {
                return null;
            }

            return _handlers[commandName].SingleOrDefault(s => s.HandlerType == handlerType);

        }

        public bool HasSubscriptionsForCommand<C, R>() where C : ICommand<R>
        {
            var key = GetCommandKey<C>();
            return HasSubscriptionsForCommand(key);
        }

        public bool HasSubscriptionsForCommand(string commandName) => (_handlers.ContainsKey(commandName) && _handlers[commandName].Count > 0) || (_handlers.ContainsKey("*") && _handlers["*"].Count > 0);

        public Type GetCommandTypeByName(string commandName) => _commandTypes.SingleOrDefault(t => t.Name == commandName);

        public string GetCommandKey<C>()
        {
            return GetCommandKey(typeof(C));
        }

        private string GetCommandKey(Type commandType)
        {
            return commandType.Name;
        }
    }
}
