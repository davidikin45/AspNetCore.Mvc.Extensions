using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersTyped
{
    public abstract class QueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    {
        Task<Result<TResult>> IQueryHandler<TQuery, TResult>.HandleAsync(string queryName, TQuery query, CancellationToken cancellationToken)
          => Task.FromResult(Handle(queryName, query));

        protected abstract Result<TResult> Handle(string queryName, TQuery query);
    }
}
