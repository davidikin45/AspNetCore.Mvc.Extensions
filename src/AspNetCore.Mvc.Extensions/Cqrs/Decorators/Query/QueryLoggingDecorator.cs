using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class QueryLoggingDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    {
        private readonly ILogger<TQuery> _logger;
        private readonly IQueryHandler<TQuery, TResult> _handler;

        public QueryLoggingDecorator(IQueryHandler<TQuery, TResult> handler, ILogger<TQuery> logger)
        {
            _logger = logger;
            _handler = handler;
        }

        public Task<Result<TResult>> HandleAsync(string commandName, TQuery query, CancellationToken cancellationToken = default)
        {
            string queryJson = JsonConvert.SerializeObject(query);

            // Use proper logging here
            _logger.LogInformation($"Query of type {query.GetType().Name}: {queryJson}");

            return _handler.HandleAsync(commandName, query, cancellationToken);
        }
    }
}
