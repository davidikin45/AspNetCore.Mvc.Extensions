using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Logging
{
    public class TrackActionPerformanceFilterAttribute : TypeFilterAttribute
    {
        public TrackActionPerformanceFilterAttribute()
            : base(typeof(TrackActionPerformanceFilterAttributeImpl))
        {

        }
        private class TrackActionPerformanceFilterAttributeImpl : ActionFilterAttribute
        {
            private readonly ILogger<TrackActionPerformanceFilterAttribute> _logger;
            private readonly Stopwatch _timer;
            private IDisposable _userScope;

            public TrackActionPerformanceFilterAttributeImpl(ILogger<TrackActionPerformanceFilterAttribute> logger)
            {
                _logger = logger;
                _timer = new Stopwatch();
            }

            public override void OnActionExecuting(ActionExecutingContext context)
            {
                _timer.Start();

                var userDict = new Dictionary<string, string>
                {
                    { "UserId", context.HttpContext.User.Claims.FirstOrDefault(a => a.Type == "sub")?.Value },
                    { "OAuth2 Scopes", string.Join(",",
                            context.HttpContext.User.Claims.Where(c => c.Type == "scope")?.Select(c => c.Value)) }
                };
                _userScope = _logger.BeginScope(userDict);
            }

            public override void OnActionExecuted(ActionExecutedContext context)
            {
                _timer.Stop();
                if (context.Exception == null)
                {
                    _logger.LogRoutePerformance(context.HttpContext.Request.Path,
                        context.HttpContext.Request.Method,
                        _timer.ElapsedMilliseconds);
                }
                _userScope?.Dispose();
            }
        }

    }
}
