using AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents
{
    public static class IntegrationEventsServiceCollectionExtensions
    {
        public static void AddInMemoryIntegrationEvents(this IServiceCollection services)
        {
            services.AddInMemoryIntegrationEventBus();
            services.AddIntegrationEventHandlers(new List<Assembly>() { AssemblyHelper.GetEntryAssembly() });
        }

        public static void AddInMemoryIntegrationEvents(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddInMemoryIntegrationEventBus();
            services.AddIntegrationEventHandlers(assemblies);
        }

        public static void AddHangFireIntegrationEvents(this IServiceCollection services)
        {
            services.AddHangFireIntegrationEventBus();
            services.AddIntegrationEventHandlers(new List<Assembly>() { AssemblyHelper.GetEntryAssembly() });
        }

        public static void AddHangFireIntegrationEvents(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddHangFireIntegrationEventBus();
            services.AddIntegrationEventHandlers(assemblies);
        }

        public static void AddRabbitMQIntegrationEvents(this IServiceCollection services, string hostName, string userName, string password, string subscriptionClientName, int retryCount = 5)
        {
            services.AddRabbitMQPersistentConnection(hostName, userName, password, retryCount);
            services.AddRabbitMQIntegrationEventBus(subscriptionClientName, retryCount);
            services.AddIntegrationEventHandlers(new List<Assembly>() { AssemblyHelper.GetEntryAssembly() });
        }

        public static void AddRabbitMQIntegrationEvents(this IServiceCollection services, IEnumerable<Assembly> assemblies, string hostName, string userName, string password, string subscriptionClientName, int retryCount = 5)
        {
            services.AddRabbitMQPersistentConnection(hostName, userName, password, retryCount);
            services.AddRabbitMQIntegrationEventBus(subscriptionClientName, retryCount);
            services.AddIntegrationEventHandlers(assemblies);
        }

        public static void AddRabbitMQPersistentConnection(this IServiceCollection services, string hostName, string userName, string password, int retryCount)
        {
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = hostName
                };

                if (!string.IsNullOrEmpty(userName))
                {
                    factory.UserName = userName;
                }

                if (!string.IsNullOrEmpty(password))
                {
                    factory.Password = password;
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });
        }

        public static void AddRabbitMQIntegrationEventBus(this IServiceCollection services, string subscriptionClientName, int retryCount)
        {
            services.AddSingleton<IIntegrationEventBus, IntegrationEventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = sp.GetRequiredService<ILogger<IntegrationEventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IIntegrationEventBusSubscriptionsManager>();

                return new IntegrationEventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });
        }

        public static void AddHangFireIntegrationEventBus(this IServiceCollection services, Action<IntegrationEventBusHangFireOptions> setupAction = null)
        {
            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton<IIntegrationEventBus, IntegrationEventBusHangFire>();
        }

        public static void AddInMemoryIntegrationEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IIntegrationEventBus, IntegrationEventBusInMemory>();
        }


        public static void AddIntegrationEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            List<Type> integrationEventHandlerTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.GetInterfaces().Any(y => IsHandlerInterface(y)))
                .Where(x => x.Name.EndsWith("Handler") && !x.IsAbstract && !x.IsGenericType)
                .ToList();

            foreach (Type integrationEventHandlerType in integrationEventHandlerTypes)
            {
                AddHandlerAsService(services, integrationEventHandlerType);
            }

            services.AddSingleton<IIntegrationEventBusSubscriptionsManager>(sp => {
                var subManager = new IntegrationEventBusInMemorySubscriptionsManager();

                //Olny typed Integration Events Autowired.
                foreach (Type integrationEventHandlerType in integrationEventHandlerTypes.Where(x => x.GetInterfaces().Any(y => IsIntegrationEventHandlerInterface(y))))
                {
                    IEnumerable<Type> interfaceTypes = integrationEventHandlerType.GetInterfaces().Where(y => IsIntegrationEventHandlerInterface(y));
                    foreach (var interfaceType in interfaceTypes)
                    {
                        Type eventType = interfaceType.GetGenericArguments()[0];

                        if(eventType != typeof(Object))
                        {
                            var eventName = subManager.GetEventKey(eventType);

                            subManager.AddSubscription(eventType, interfaceType);
                        }
                    }
                }
                return subManager;
            });
        }

        private static void AddHandlerAsService(IServiceCollection services, Type type)
        {
            List<Type> pipeline = GetPipeline(type);

            IEnumerable<Type> interfaceTypes = type.GetInterfaces().Where(y => IsHandlerInterface(y));

            foreach (var interfaceType in interfaceTypes)
            {
                Func<IServiceProvider, object> factory = BuildPipeline(pipeline, interfaceType);

                Type eventType = interfaceType.GetGenericArguments()[0];
                if(eventType == typeof(Object))
                {
                    services.TryAddTransient(type, factory);
                }
                else
                {
                    services.AddTransient(interfaceType, factory);
                }
            }
        }

        private static List<Type> GetPipeline(Type type)
        {
            object[] attributes = type.GetCustomAttributes(false);

            List<Type> pipeline = attributes
                .Select(x => ToDecorator(x))
                .Concat(new[] { type })
                .Reverse()
                .ToList();

            return pipeline;
        }

        private static Func<IServiceProvider, object> BuildPipeline(List<Type> pipeline, Type interfaceType)
        {
            List<ConstructorInfo> ctors = pipeline
                .Select(x =>
                {
                    Type type = x.IsGenericType ? x.MakeGenericType(interfaceType.GenericTypeArguments) : x;
                    return type.GetConstructors().Single();
                })
                .ToList();

            Func<IServiceProvider, object> func = provider =>
            {
                object current = null;

                foreach (ConstructorInfo ctor in ctors)
                {
                    List<ParameterInfo> parameterInfos = ctor.GetParameters().ToList();

                    object[] parameters = GetParameters(parameterInfos, current, provider);

                    current = ctor.Invoke(parameters);
                }

                return current;
            };

            return func;
        }

        private static object[] GetParameters(List<ParameterInfo> parameterInfos, object current, IServiceProvider provider)
        {
            var result = new object[parameterInfos.Count];

            for (int i = 0; i < parameterInfos.Count; i++)
            {
                result[i] = GetParameter(parameterInfos[i], current, provider);
            }

            return result;
        }

        private static object GetParameter(ParameterInfo parameterInfo, object current, IServiceProvider provider)
        {
            Type parameterType = parameterInfo.ParameterType;

            if (IsHandlerInterface(parameterType))
                return current;

            object service = provider.GetService(parameterType);
            if (service != null)
                return service;

            throw new ArgumentException($"Type {parameterType} not found");
        }

        private static Type ToDecorator(object attribute)
        {
            Type type = attribute.GetType();


            throw new ArgumentException(attribute.ToString());
        }

        private static bool IsHandlerInterface(Type type)
        {
            return IsIntegrationEventHandlerInterface(type);
        }

        private static bool IsIntegrationEventHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefinition = type.GetGenericTypeDefinition();

            return typeDefinition == typeof(IIntegrationEventHandler<>);
        }

        //private void ConfigureEventBus(IApplicationBuilder app)
        //{
        //    var eventBus = app.Application.GetRequiredService<IEventBus>();
        //    eventBus.Subscribe<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
        //}
    }
}
