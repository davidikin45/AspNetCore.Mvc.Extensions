using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.IntegrationEvents.Subscriptions;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents
{
    //Each microservice has a seperate queue 
    //Publisher > Exchange > Queue > Consumer
    public class IntegrationEventBusRabbitMQ : IIntegrationEventBus, IDisposable
    {

        const string BROKER_NAME = "event_bus";

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<IntegrationEventBusRabbitMQ> _logger;
        private readonly IIntegrationEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retryCount;

        private IModel _consumerChannel;
        private string _queueName;

        public IntegrationEventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<IntegrationEventBusRabbitMQ> logger,
            IServiceProvider serviceProvider, IIntegrationEventBusSubscriptionsManager subsManager, string queueName = null, int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new IntegrationEventBusInMemorySubscriptionsManager();
            _queueName = queueName;
            _consumerChannel = CreateConsumerChannel();
            _serviceProvider = serviceProvider;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: BROKER_NAME,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        public Task PublishAsync(IntegrationEvent integrationEvent)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex.ToString());
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = integrationEvent.GetType()
                    .Name;

                //Explicit Exhange.
                channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

                var message = JsonConvert.SerializeObject(integrationEvent);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent
                    //properties.ReplyTo = "_replyQueueName";
                    //properties.CorrelationId = Guid.NewGuid().ToString();

                    channel.BasicPublish(exchange: BROKER_NAME,
                                     routingKey: eventName,
                                     mandatory: true,
                                     basicProperties: properties,
                                     body: body);
                });
            }

            return Task.CompletedTask;
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>
        {
            DoInternalSubscription(eventName);
            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);
            _subsManager.AddSubscription<T, TH>();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: _queueName,
                                      exchange: BROKER_NAME,
                                      routingKey: eventName);
                }
            }
        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subsManager.Clear();
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            //Explicit Exchange
            channel.ExchangeDeclare(exchange: BROKER_NAME,
                                 type: "direct"); //fanout = ignore routingkey

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var props = ea.BasicProperties;
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                await ProcessEventAsync(eventName, message);

                //var replyProps = _consumerChannel.CreateBasicProperties();
                //replyProps.CorrelationId = props.CorrelationId;
                //channel.BasicPublish(exchange: BROKER_NAME,
                //                   routingKey: props.ReplyTo,
                //                   mandatory: true,
                //                   basicProperties: replyProps,
                //                   body: body);

                //A new message won't be received by consumer until acknowledgement is sent.
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            };

            return channel;
        }

        public async Task ProcessEventAsync(string eventName, string payload)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                //Each Integration Event is a Unit of Work which could trigger many commands.
                using (var scope = _serviceProvider.CreateScope())
                {
                    scope.ServiceProvider.BeginUnitOfWork();

                    foreach (var subscription in subscriptions)
                    {
                        for (int i = 0; i < subscription.HandlerCount; i++)
                        {
                            await ProcessEventHandlerAsync(eventName, payload, subscription.HandlerType.FullName, i, scope.ServiceProvider).ConfigureAwait(false);
                        }
                    }

                    await scope.ServiceProvider.CompleteUnitOfWorkAsync();
                }
            }
        }

        public async Task ProcessEventHandlerAsync(string eventName, string payload, string handlerType, int handlerIndex, IServiceProvider serviceProvider)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscription = _subsManager.GetHandlersForEvent(eventName).FirstOrDefault(s => s.HandlerType.FullName == handlerType);
                if (subscription != null)
                {
                    dynamic integrationEvent;
                    if (subscription.IsDynamic)
                    {
                        integrationEvent = JObject.Parse(payload);
                    }
                    else
                    {
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        integrationEvent = JsonConvert.DeserializeObject(payload, eventType);
                    }

                     await DispatchEventAsync(subscription.HandlerType, handlerIndex, eventName, integrationEvent, serviceProvider).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("Invalid handler type");
                }
            }
        }

        private async Task DispatchEventAsync(Type handlerType, int handlerIndex, string eventName, dynamic integrationEvent, IServiceProvider serviceProvider)
        {
            IEnumerable<dynamic> handlers = serviceProvider.GetServices(handlerType);

            dynamic handler = handlers.Skip(handlerIndex).Take(1).First();

            if (handler == null)
            {
                throw new Exception("Invalid handler index");
            }

            Result result = await handler.HandleAsync(eventName, integrationEvent, default(CancellationToken)).ConfigureAwait(false);
            if (result.IsFailure)
            {
                throw new Exception("Integration Event Failed");
            }

            //var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            //await (Task)concreteType.GetMethod("HandleAsync").Invoke(handler, new object[] { integrationEvent, default(CancellationToken) });

        }
    }
}

