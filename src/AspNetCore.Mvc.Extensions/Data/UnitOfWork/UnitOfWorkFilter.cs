using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System.Transactions;

namespace AspNetCore.Mvc.Extensions.Data.UnitOfWork
{
    //https://medium.com/agilix/asp-net-core-one-transaction-per-server-roundtrip-baab72b41e91
    public class UnitOfWorkFilter : IAsyncActionFilter
    {
        private TransactionScope _transactionScope;

        public UnitOfWorkFilter(TransactionScope transactionScope)
        {
            _transactionScope = transactionScope;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next.Invoke();
            if (executedContext.Exception == null)
            {
                _transactionScope.Complete();
            }
            else
            {
                if (_transactionScope != null)
                {
                    _transactionScope.Dispose();
                }
                _transactionScope = null;
            }
        }
    }
}