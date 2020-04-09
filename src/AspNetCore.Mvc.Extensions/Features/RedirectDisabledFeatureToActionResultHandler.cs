using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Features
{
    public class RedirectDisabledFeatureToActionResultHandler : IDisabledFeaturesHandler
    {
        private readonly IActionResult _result;

        public RedirectDisabledFeatureToActionResultHandler(IActionResult result)
        {
            _result = result;
        }

        public Task HandleDisabledFeatures(IEnumerable<string> features, ActionExecutingContext context)
        {
            context.Result = _result;
            return Task.CompletedTask;
        }
    }
}