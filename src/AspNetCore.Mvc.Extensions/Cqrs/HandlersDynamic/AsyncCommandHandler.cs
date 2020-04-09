using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Mvc.Extensions.Validation;

namespace AspNetCore.Mvc.Extensions.Cqrs.HandlersDynamic
{

    public abstract class AsyncDynamicCommandHandler<TCommand, TResult> : IDynamicCommandHandler<TCommand, TResult>
    {
        public abstract Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default);
    }

    public abstract class AsyncDynamicCommandHandler<TCommand> : IDynamicCommandHandler<TCommand>
    {
        public async Task<Result<string>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            var result = await HandleNoReturnAsync(commandName, command, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Result.Ok<string>(null) : Result.Fail<string>(result.ErrorType.Value);
        }

        protected abstract Task<Result> HandleNoReturnAsync(string commandName, TCommand command, CancellationToken cancellationToken);

    }

    public abstract class AsyncDynamicRequestCommandHandler<TResult> : AsyncDynamicCommandHandler<dynamic, TResult>
    {
      
    }

    //No return
    public abstract class AsyncDynamicRequestCommandHandler : AsyncDynamicCommandHandler<dynamic>
    {


    }

    public abstract class AsyncDynamicRequestResponseCommandHandler : AsyncDynamicCommandHandler<dynamic, object>
    {

    }
}
