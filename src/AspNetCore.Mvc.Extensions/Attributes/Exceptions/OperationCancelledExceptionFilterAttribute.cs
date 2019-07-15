using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Exceptions
{
    public class OperationCancelledExceptionFilterAttribute : TypeFilterAttribute
    {
        public OperationCancelledExceptionFilterAttribute()
            : base(typeof(OperationCancelledExceptionFilterImpl))
        {

        }
        private class OperationCancelledExceptionFilterImpl : ExceptionFilterAttribute
        {
            private readonly ILogger _logger;

            public OperationCancelledExceptionFilterImpl(ILoggerFactory loggerFactory)
            {
                _logger = loggerFactory.CreateLogger<OperationCancelledExceptionFilterAttribute>();
            }
            public override void OnException(ExceptionContext context)
            {
                HandleException(context);
            }

            private void HandleException(ExceptionContext context)
            {
                if (context.Exception is OperationCanceledException)
                {
                    _logger.LogInformation("Request was cancelled");
                    context.ExceptionHandled = true;
                    context.Result = new StatusCodeResult(400);
                }
            }
        }

    }
}
