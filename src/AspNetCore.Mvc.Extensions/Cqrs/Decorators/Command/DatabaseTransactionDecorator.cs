using AspNetCore.Mvc.Extensions.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class DatabaseTransactionDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> _handler;
        private readonly TransactionScope _transactionScope;

        public DatabaseTransactionDecorator(ICommandHandler<TCommand, TResult> handler)
        {
            _handler = handler;
            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });
        }

        public async Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                Result<TResult> result = await _handler.HandleAsync(commandName, command, cancellationToken);

                _transactionScope.Complete();

                return result;
            }
            catch (Exception)
            {
                if (_transactionScope != null)
                {
                    _transactionScope.Dispose();
                }
                throw;
            }
        }
    }
}
