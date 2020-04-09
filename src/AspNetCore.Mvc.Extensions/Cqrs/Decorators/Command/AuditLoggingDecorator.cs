using AspNetCore.Mvc.Extensions.Validation;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class AuditLoggingDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> _handler;

        public AuditLoggingDecorator(ICommandHandler<TCommand, TResult> handler)
        {
            _handler = handler;
        }

        public Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            string commandJson = JsonConvert.SerializeObject(command);

            // Use proper logging here
            Console.WriteLine($"Command of type {command.GetType().Name}: {commandJson}");

            return _handler.HandleAsync(commandName, command, cancellationToken);
        }
    }
}
