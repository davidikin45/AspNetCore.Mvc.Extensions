using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersTyped
{
    public abstract class AsyncQueryHandler<TQuery, TResult> : ITypedQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        public abstract Task<Result<TResult>> HandleAsync(string queryName, TQuery query, CancellationToken cancellationToken = default);
    }
}
