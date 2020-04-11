using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Logging
{
    public class ViewLocationsLogger : IHostedService
    {
        private readonly ILogger<ViewLocationsLogger> _logger;
        private readonly RazorViewEngineOptions _options;

        public ViewLocationsLogger(ILogger<ViewLocationsLogger> logger, IOptions<RazorViewEngineOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("View Locations:" + Environment.NewLine + string.Join(Environment.NewLine, _options.ViewLocationFormats));
            _logger.LogInformation("Page View Locations:" + Environment.NewLine + string.Join(Environment.NewLine, _options.PageViewLocationFormats));
            _logger.LogInformation("Area View Locations:" + Environment.NewLine + string.Join(Environment.NewLine, _options.AreaViewLocationFormats));
            _logger.LogInformation("Area Page View Locations:" + Environment.NewLine + string.Join(Environment.NewLine, _options.AreaPageViewLocationFormats));

            return Task.CompletedTask;
        }

        // Required by the interface
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
