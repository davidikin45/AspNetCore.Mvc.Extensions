using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersDynamic
{

    public abstract class DynamicCommandHandler<TCommand, TResult> : IDynamicCommandHandler<TCommand, TResult>
    {
        public virtual Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken)
          => Task.FromResult(Handle(commandName, command));

        protected abstract Result<TResult> Handle(string commandName, TCommand command);
    }

    public abstract class DynamicCommandHandler<TCommand> : IDynamicCommandHandler<TCommand>
    {
        public virtual Task<Result<string>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            var result = HandleNoReturn(commandName, command);
            return Task.FromResult(result.IsSuccess ? Result.Ok<string>(null) : Result.Fail<string>(result.ErrorType.Value));
        }

        protected abstract Result HandleNoReturn(string commandName, TCommand command);
    }


    public abstract class DynamicRequestCommandHandler<TResult> : DynamicCommandHandler<dynamic, TResult>
    {

    }

    public abstract class DynamicRequestResponseCommandHandler : DynamicCommandHandler<dynamic, dynamic>
    {

    }
}
