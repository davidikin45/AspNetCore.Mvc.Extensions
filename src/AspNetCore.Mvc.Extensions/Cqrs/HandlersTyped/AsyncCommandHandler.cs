using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersTyped
{
    public abstract class AsyncCommandHandler<TCommand, TResult> : ITypedCommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        public abstract Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default);

    }

    //No return
    public abstract class AsyncCommandHandler<TCommand> : ITypedCommandHandler<TCommand>
          where TCommand : ICommand
    {
        public async Task<Result<string>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken)
        {
            var result = await HandleNoReturnAsync(commandName, command, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Result.Ok<string>(null) : Result.Fail<string>(result.ErrorType.Value);
        }

        protected abstract Task<Result> HandleNoReturnAsync(string commandName, TCommand command, CancellationToken cancellationToken);
    }
}