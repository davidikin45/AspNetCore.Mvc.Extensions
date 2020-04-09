using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class DatabaseRetryDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> _handler;

        public DatabaseRetryDecorator(ICommandHandler<TCommand, TResult> handler)
        {
            _handler = handler;
        }

        public async Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            for (int i = 0; ; i++)
            {
                try
                {
                    Result<TResult> result = await _handler.HandleAsync(commandName, command, cancellationToken);
                    return result;
                }
                catch (Exception ex)
                {
                    if (i >= 3 || !IsDatabaseException(ex))
                        throw;
                }
            }
        }

        private bool IsDatabaseException(Exception exception)
        {
            string message = exception.InnerException?.Message;

            if (message == null)
                return false;

            return message.Contains("The connection is broken and recovery is not possible")
                || message.Contains("error occurred while establishing a connection");
        }
    }
}
