using AspNetCore.Mvc.Extensions.Cqrs.Decorators;
using AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command;
using AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Cqrs
{
    public static class CqrsServiceCollectionExtensions
    {
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddCqrs(this IServiceCollection services)
        {
            services.AddCqrsMediator();
            services.AddCqrsHandlers(new List<Assembly>() { AssemblyHelper.GetEntryAssembly() });
        }

        public static void AddCqrs(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddCqrsMediator();
            services.AddCqrsHandlers(assemblies);
        }

        public static void AddCqrsMediator(this IServiceCollection services)
        {
            services.TryAddTransient<ICqrsMediator, CqrsMediator>();
        }

        public static void AddCqrsHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            bool addHandlersAsServices = false;

            //commands
            List<Type> commandHandlerTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.GetInterfaces().Any(y => IsCommandHandlerInterface(y)))
                .Where(x => x.Name.EndsWith("Handler") && !x.IsAbstract && !x.IsGenericType)
                .ToList();

            //Add Handlers as Transient Services to DI
            //ICommandHandler<dynamic,TResponse> are registered against concrete Type.
            //ICommandHandler<TCommand,TResponse> are registered against interface.
            if(addHandlersAsServices)
            {
                foreach (Type commandHandlerType in commandHandlerTypes)
                {
                    AddHandlerAsService(services, commandHandlerType, false);
                }
            }

            //Register non dynamic Command Subscription. Dynamic subscriptions need to be added manually.
            //TCommand --> ICommandHandler<TCommand,TResponse>
            services.AddSingleton<ICqrsCommandSubscriptionsManager>(sp => {
                var subManager = new CqrsInMemoryCommandSubscriptionsManager();
                foreach (Type commandHandlerType in commandHandlerTypes.Where(x => x.GetInterfaces().Any(y => IsCommandHandlerInterface(y))))
                {
                    IEnumerable<Type> interfaceTypes = commandHandlerType.GetInterfaces().Where(y => IsCommandHandlerInterface(y));

                    foreach (Type interfaceType in interfaceTypes)
                    {
                        Type commandType = interfaceType.GetGenericArguments()[0];

                        if (commandType != typeof(Object))
                        {
                            if(addHandlersAsServices)
                            {
                                subManager.AddSubscription(commandType, interfaceType);
                            }
                            else
                            {
                                subManager.AddSubscription(commandType, commandHandlerType);
                            }
                        }
                    }
                }
                return subManager;
            });

            //queries
            List<Type> queryHandlerTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.GetInterfaces().Any(y => IsQueryHandlerInterface(y)))
                .Where(x => x.Name.EndsWith("Handler") && !x.IsAbstract && !x.IsGenericType)
                .ToList();

            //Add Handlers as Transient Services to DI
            //IQueryHandler<dynamic,TResult> are registered against concrete Type.
            //IQueryHandler<TQuery,TResult> are registered against interface.
            if(addHandlersAsServices)
            {
                foreach (Type queryHandlerType in queryHandlerTypes)
                {
                    AddHandlerAsService(services, queryHandlerType, true);
                }
            }

            //Register non dynamic Query Subscription. Dynamic subscriptions need to be added manually.
            //TQuery --> IQueryHandler<TQuery,TResult>
            services.AddSingleton<ICqrsQuerySubscriptionsManager>(sp => {
                var subManager = new CqrsInMemoryQuerySubscriptionsManager();
                foreach (Type queryHandlerType in queryHandlerTypes.Where(x => x.GetInterfaces().Any(y => IsQueryHandlerInterface(y))))
                {
                    IEnumerable<Type> interfaceTypes = queryHandlerType.GetInterfaces().Where(y => IsQueryHandlerInterface(y));

                    foreach (var interfaceType in interfaceTypes)
                    {
                        Type queryType = interfaceType.GetGenericArguments()[0];

                        if(queryType != typeof(Object))
                        {
                            if (addHandlersAsServices)
                            {
                                subManager.AddSubscription(queryType, interfaceType);
                            }
                            else
                            {
                                subManager.AddSubscription(queryType, queryHandlerType);
                            }
                        }
                    }
                }
                return subManager;
            });
        }

        public static Func<IServiceProvider, object> CreateFactory(Type commandOrQueryType, Type handlerType, bool isQuery)
        {
            var pipeline = GetPipeline(handlerType, isQuery);

            //ICommandHandler<TCommand,TResponse> OR IQueryHandler<TQuery,TResult>
            var handlerInterfaceType = handlerType.GetInterfaces().Where(y => IsHandlerInterface(y) && (commandOrQueryType == null || commandOrQueryType == typeof(Object) || y.GetGenericArguments()[0] == commandOrQueryType)).First();

            Func<IServiceProvider, object> factory = BuildPipeline(pipeline, handlerInterfaceType);

            return factory;
        }

        //No longer used
        private static void AddHandlerAsService(IServiceCollection services, Type type, bool isQuery)
        {
            var pipeline = GetPipeline(type, isQuery);

            //ICommandHandler<TCommand,TResponse> OR IQueryHandler<TQuery,TResult>
            IEnumerable<Type> interfaceTypes = type.GetInterfaces().Where(y => IsHandlerInterface(y));

            //Build Factory for each ICommandHandler<TCommand,TResponse>, IQueryHandler<TQuery,TResult> on the Concrete Type.
            foreach (var interfaceType in interfaceTypes)
            {
                Func<IServiceProvider, object> factory = BuildPipeline(pipeline, interfaceType);

                Type commandOrQueryType = interfaceType.GetGenericArguments()[0];
                if (commandOrQueryType == typeof(Object))
                {
                    //dynamic
                    //TCommand OR TQuery = dynamic(object) where ICommandHandler<TCommand,TResponse>, IQueryHandler<TQuery,TResult>
                    //Register against concrete type

                    //When decorating concrete types it causes issues as the object returned from the factory is not the concrete type!
                    services.TryAddTransient(type, factory);
                }
                else
                {
                    //Decorator pattern
                    services.AddTransient(interfaceType, factory);
                }
            }
        }

        //ICqrsDecoratorFactory
        private static List<CqrsDecoratorAttribute> GetPipeline(Type handlerType, bool isQuery)
        {
            //Decorator Pattern by binding to handler interface instead of concrete type.
            object[] attributes = handlerType.GetCustomAttributes(false);

            List<CqrsDecoratorAttribute> pipeline;
            if (isQuery)
            {
                pipeline = attributes
               .Select(x => ToDecorator(x)).Where(x => x != null)
               .Concat(new[] { new ReadOnlyAttribute()})
               .Concat(new[] { new QueryValidatorAttribute() })
               .Concat(new[] { new CqrsDecoratorAttribute(handlerType) })
               .Reverse()
               .ToList();
            }
            else
            {
                pipeline = attributes
               .Select(x => ToDecorator(x)).Where(x => x!= null)
               .Concat(new[] { new CommandValidatorAttribute() })
               .Concat(new[] { new CqrsDecoratorAttribute(handlerType) })
               .Reverse()
               .ToList();
            }

            return pipeline;
        }

        private static Func<IServiceProvider, object> BuildPipeline(List<CqrsDecoratorAttribute> pipeline, Type interfaceType)
        {
            var typeAndArgsList = pipeline
                .Select(x =>
                {
                    Type type = x.ImplementationType.IsGenericType ? x.ImplementationType.MakeGenericType(interfaceType.GenericTypeArguments) : x.ImplementationType;
                    return new { Type = type, x.Arguments};
                })
                .ToList();

            //Factory
            Func<IServiceProvider, object> func = provider =>
            {
                object current = null;

                //First ctor is the actual handler
                //Wrap Handler
                foreach (var typeAndArgs in typeAndArgsList)
                {
                    current = CreateInstance(provider, typeAndArgs.Type, current, typeAndArgs.Arguments);
                }

                return current;
            };

            return func;
        }

        public static object CreateInstance(IServiceProvider serviceProvider, Type handlerType, object handlerToWrap, params object[] ctorArgs)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var argsList = new List<object>();

            if (handlerToWrap != null)
            {
                argsList.Add(handlerToWrap);
            }

            if (ctorArgs != null)
                argsList.AddRange(ctorArgs);

            var args = argsList.ToArray();

            var handler = ActivatorUtilities.CreateInstance(serviceProvider, handlerType, args);

            return handler;
        }

        private static CqrsDecoratorAttribute ToDecorator(object attribute)
        {
            if (attribute is CqrsDecoratorAttribute cqrsDecoratorAttribute)
                return cqrsDecoratorAttribute;

            return null;
        }
        private static bool IsHandlerInterface(Type type)
        {
            return IsCommandHandlerInterface(type)  || IsQueryHandlerInterface(type);
        }

        private static bool IsCommandHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefinition = type.GetGenericTypeDefinition();

            return typeDefinition == typeof(ICommandHandler<,>);
        }

        private static bool IsQueryHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefinition = type.GetGenericTypeDefinition();

            return typeDefinition == typeof(IQueryHandler<,>);
        }
    }
}
