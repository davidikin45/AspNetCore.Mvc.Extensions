using AspNetCore.Mvc.Extensions.DomainEvents;
using AspNetCore.Mvc.Extensions.DomainEvents.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Cqrs
{
    public static class DomainEventsServiceCollectionExtensions
    {
        public static void AddInMemoryDomainEvents(this IServiceCollection services)
        {
            services.AddInMemoryDomainEventBus();
            services.AddDomainEventHandlers(new List<Assembly>() { AssemblyHelper.GetEntryAssembly() });
        }

        public static void AddInMemoryDomainEvents(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddInMemoryDomainEventBus();
            services.AddDomainEventHandlers(assemblies);
        }
        public static void AddHangFireDomainEvents(this IServiceCollection services)
        {
            services.AddHangFireDomainEventBus();
            services.AddDomainEventHandlers(new List<Assembly>() { AssemblyHelper.GetEntryAssembly() });
        }

        public static void AddHangFireDomainEvents(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddHangFireDomainEventBus();
            services.AddDomainEventHandlers(assemblies);
        }

        public static void AddHangFireDomainEventBus(this IServiceCollection services, Action<DomainEventBusHangFireOptions> setupAction = null)
        {
            if (setupAction != null)
                services.Configure(setupAction);

            services.TryAddTransient<IDomainEventBus, DomainEventBusHangFire>();
        }

        public static void AddInMemoryDomainEventBus(this IServiceCollection services)
        {
            services.TryAddTransient<IDomainEventBus, DomainEventBusInMemory>();
        }

        //Registering as Interface allows decorator pattern but means can't regier two handlers for an event.
        public static void AddDomainEventHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            List<Type> domainEventHandlerTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.GetInterfaces().Any(y => IsHandlerInterface(y)))
                .Where(x => x.Name.EndsWith("Handler") && !x.IsAbstract && !x.IsGenericType)
                .ToList();

            foreach (Type domainEventHandlerType in domainEventHandlerTypes)
            {
                AddHandlerAsService(services, domainEventHandlerType);
            }

            services.AddSingleton<IDomainEventBusSubscriptionsManager>(sp => {
                var subManager = new DomainEventBusInMemorySubscriptionsManager();
                foreach (Type domainEventHandlerType in domainEventHandlerTypes.Where(x => x.GetInterfaces().Any(y => IsDomainEventHandlerInterface(y))))
                {
                    IEnumerable<Type> interfaceTypes = domainEventHandlerType.GetInterfaces().Where(y => IsDomainEventHandlerInterface(y));

                    foreach (var interfaceType in interfaceTypes)
                    {
                        Type domainEventType = interfaceType.GetGenericArguments()[0];
                        if(domainEventType != typeof(Object))
                        {
                            subManager.AddSubscription(domainEventType, interfaceType);
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

                Type domainEventType = interfaceType.GetGenericArguments()[0];
                if (domainEventType == typeof(Object))
                {
                    services.TryAddTransient(type, factory);
                }
                else
                {
                    //Decorator pattern
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
            if (!type.IsGenericType)
                return false;

            return IsDomainEventHandlerInterface(type);
        }

        private static bool IsDomainEventHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefinition = type.GetGenericTypeDefinition();

            return typeDefinition == typeof(IDomainEventHandler<>);
        }
    }
}
