using AspNetCore.Mvc.Extensions.Cqrs.HandlersDynamic;
using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class ReadOnlyDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    { 
        private readonly IQueryHandler<TQuery, TResult> _handler;
        private readonly IUnitOfWork[] _unitOfWorks;

        public ReadOnlyDecorator(IQueryHandler<TQuery, TResult> handler, IUnitOfWork[] unitOfWorks)
        {
            _handler = handler;
            _unitOfWorks = unitOfWorks;
        }

        public async Task<Result<TResult>> HandleAsync(string queryName, TQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                _unitOfWorks.ToList().ForEach(uow => uow.AutoDetectChangesEnabled = false);
                _unitOfWorks.ToList().ForEach(uow => uow.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking);

                Result<TResult> result = await _handler.HandleAsync(queryName, query, cancellationToken);
                return result;
            }
            finally
            {
                _unitOfWorks.ToList().ForEach(uow => uow.AutoDetectChangesEnabled = true);
                _unitOfWorks.ToList().ForEach(uow => uow.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll);
            }
        }
    }
}
