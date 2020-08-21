using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class CommandLoggingDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        private readonly ILogger<TCommand> _logger;
        private readonly ICommandHandler<TCommand, TResult> _handler;

        public CommandLoggingDecorator(ICommandHandler<TCommand, TResult> handler, ILogger<TCommand> logger)
        {
            _logger = logger;
            _handler = handler;
        }

        public Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            string commandJson = JsonConvert.SerializeObject(command);

            // Use proper logging here
            _logger.LogInformation($"Command of type {command.GetType().Name}: {commandJson}");

            return _handler.HandleAsync(commandName, command, cancellationToken);
        }
    }
}
