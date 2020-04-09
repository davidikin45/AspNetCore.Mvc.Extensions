using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.DomainEvents.HandlersTyped
{
    public abstract class DomainEventHandler<TDomainEvent> : ITypedDomainEventHandler<TDomainEvent>
     where TDomainEvent : DomainEvent
    {
        public Task<Result> HandlePreCommitAsync(string eventName, TDomainEvent domainEvent, CancellationToken cancellationToken)
            => Task.FromResult(HandlePreCommit(eventName, domainEvent));

        protected abstract Result HandlePreCommit(string eventName, TDomainEvent domainEvent);

        public Task<Result> HandlePostCommitAsync(string eventName, TDomainEvent domainEvent, CancellationToken cancellationToken)
          => Task.FromResult(HandlePostCommit(eventName, domainEvent));

        protected abstract Result HandlePostCommit(string eventName, TDomainEvent domainEvent);

     

    }
}
