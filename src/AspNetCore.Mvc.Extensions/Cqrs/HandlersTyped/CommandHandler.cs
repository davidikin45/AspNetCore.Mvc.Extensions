using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersTyped
{
    public abstract class CommandHandler<TCommand, TResult> : ITypedCommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        public virtual Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken)
          => Task.FromResult(Handle(commandName, command));

        protected abstract Result<TResult> Handle(string commandName, TCommand command);
    }

    public abstract class CommandHandler<TCommand> : ITypedCommandHandler<TCommand>
        where TCommand: ICommand
    {
        public Task<Result<string>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken)
        {
            var result = HandleNoReturn(commandName, command);
            return Task.FromResult(result.IsSuccess ? Result.Ok<string>(null) : Result.Fail<string>(result.ErrorType.Value));
        }

        protected abstract Result HandleNoReturn(string commandName, TCommand command);
    }
}