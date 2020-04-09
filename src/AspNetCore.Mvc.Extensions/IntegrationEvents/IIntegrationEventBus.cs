using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.IntegrationEvents
{
    public interface IIntegrationEventBus
    {
        Task PublishAsync(IntegrationEvent integrationEvent);

        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        void SubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler<object>;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler<object>;

        Task ProcessEventAsync(string eventName, string payload);

        //Task ProcessEventHandlerAsync(string eventName, string payload, string handlerType, int handlerIndex);
    }
}
