using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Mvc.Extensions.Validation;

namespace AspNetCore.Mvc.Extensions.DomainEvents.HandlersDynamic
{
    public abstract class AsyncDynamicDomainEventHandler<TDomainEvent> : IDynamicDomainEventHandler<TDomainEvent>
    {
        public abstract Task<Result> HandlePostCommitAsync(string eventName, TDomainEvent domainEvent, CancellationToken cancellationToken = default);

        public abstract Task<Result> HandlePreCommitAsync(string eventName, TDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }

    public abstract class AsyncDynamicRequestDomainEventHandler: AsyncDynamicDomainEventHandler<dynamic>
    {

    }
}
