using AspNetCore.Mvc.Extensions.DomainEvents.HandlersDynamic;
using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace TemplateAspNetCore3.DomainEvents
{
    public class DomainEventHandler : AsyncDynamicRequestDomainEventHandler
    {
        public override Task<Result> HandlePostCommitAsync(string eventName, dynamic domainEvent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok());
        }

        public override Task<Result> HandlePreCommitAsync(string eventName, dynamic domainEvent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok());
        }
    }
}
