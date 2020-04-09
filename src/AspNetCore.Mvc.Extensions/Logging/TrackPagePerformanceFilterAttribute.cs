using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AspNetCore.Mvc.Extensions.Logging
{

    public class TrackPagePerformanceFilterAttribute : TypeFilterAttribute
    {
        public TrackPagePerformanceFilterAttribute()
            : base(typeof(TrackPagePerformanceFilterImp))
        {

        }
        private class TrackPagePerformanceFilterImp : IPageFilter
        {
            private readonly ILogger<TrackPagePerformanceFilterAttribute> _logger;
            private Stopwatch _timer;

            public TrackPagePerformanceFilterImp(ILogger<TrackPagePerformanceFilterAttribute> logger)
            {
                _logger = logger;
            }
            public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
            {
                _timer = new Stopwatch();
                _timer.Start();
            }

            public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
            {
                _timer.Stop();
                if (context.Exception == null)
                {
                    _logger.LogRoutePerformance(context.ActionDescriptor.RelativePath,
                        context.HttpContext.Request.Method,
                        _timer.ElapsedMilliseconds);
                }
                //_logger.LogInformation("{PageName} {Method} model code took {ElapsedMilliseconds}.",
                //    context.ActionDescriptor.RelativePath, 
                //    context.HttpContext.Request.Method, 
                //    _timer.ElapsedMilliseconds);
            }

            public void OnPageHandlerSelected(PageHandlerSelectedContext context)
            {
                // not needed
            }
        }
    }
}
