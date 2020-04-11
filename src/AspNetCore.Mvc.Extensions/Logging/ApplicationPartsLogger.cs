using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Logging
{
    //https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/
    public class ApplicationPartsLogger : IHostedService
    {
        private readonly ILogger<ApplicationPartsLogger> _logger;
        private readonly ApplicationPartManager _partManager;

        public ApplicationPartsLogger(ILogger<ApplicationPartsLogger> logger, ApplicationPartManager partManager)
        {
            _logger = logger;
            _partManager = partManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Get the names of all the application parts. This is the short assembly name for AssemblyParts
            var applicationParts = _partManager.ApplicationParts.Select(x => x.Name);

            var controllerFeature = new ControllerFeature();
            _partManager.PopulateFeature(controllerFeature);
            var controllers = controllerFeature.Controllers.Select(x => x.Name);

            var tagHelperFeature = new TagHelperFeature();
            _partManager.PopulateFeature(tagHelperFeature);
            var tagHelpers = tagHelperFeature.TagHelpers.Select(x => x.Name);

            var viewComponentFeature = new ViewComponentFeature();
            _partManager.PopulateFeature(viewComponentFeature);
            var viewComponents = viewComponentFeature.ViewComponents.Select(x => x.Name);

            var viewsFeature = new ViewsFeature();
            _partManager.PopulateFeature(viewsFeature);
            var views = viewsFeature.ViewDescriptors.Select(x => x.RelativePath);

            //Log the application parts
            _logger.LogInformation("Found the following Application Parts: " + Environment.NewLine + string.Join(Environment.NewLine, applicationParts));
            _logger.LogInformation("Found the following Controllers: " + Environment.NewLine + string.Join(Environment.NewLine, controllers));
            _logger.LogInformation("Found the following Views: " + Environment.NewLine + string.Join(Environment.NewLine, views));
            _logger.LogInformation("Found the following Tag Helpers: " + Environment.NewLine + string.Join(Environment.NewLine, tagHelpers));
            _logger.LogInformation("Found the following View Components: " + Environment.NewLine + string.Join(Environment.NewLine, viewComponents));

            return Task.CompletedTask;
        }

        // Required by the interface
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
