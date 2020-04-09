using RabbitMQ.Client;
using System;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents
{
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
