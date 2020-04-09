using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersDynamic
{
    public abstract class DynamicQueryHandler<TQuery, TResult> : IDynamicQueryHandler<TQuery, TResult>
    {
        public Task<Result<TResult>> HandleAsync(string queryName, TQuery query, CancellationToken cancellationToken)
          => Task.FromResult(Handle(queryName, query));

        protected abstract Result<TResult> Handle(string queryName, TQuery query);
    }

    public abstract class DynamicRequestQueryHandler<TResult> : DynamicQueryHandler<dynamic, TResult>
    {

    }

    public abstract class DynamicRequestResponseQueryHandler : DynamicQueryHandler<dynamic, dynamic>
    {


    }
}
