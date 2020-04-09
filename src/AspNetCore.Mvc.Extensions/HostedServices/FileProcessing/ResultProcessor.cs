using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices.FileProcessing
{
    public abstract class ResultProcessor<T> : IResultProcessor<T>
    {
        private readonly ICsvResultParser<DataRow> _csvParser;
        private readonly ILogger _logger;

        public ResultProcessor(ICsvResultParser<DataRow> csvParser, ILogger logger)
        {
            _csvParser = csvParser;
            _logger = logger;
        }

        public Task ProcessAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var results = _csvParser.ParseResult(stream);

            _logger.LogInformation($"Processing {results.Count} results.");

            // Last chance to cancel. After this point, we allow all results to be calculated and posted, even if cancellation is signaled.
            // We don't want to send results for half of a file.
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var result in results)
            {
               
            }

            return Task.CompletedTask;
        }

        public abstract Task ProcessRowAsync(Task row);
    }
}
