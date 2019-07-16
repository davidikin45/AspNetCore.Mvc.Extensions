using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Attributes.Validation
{
    //options.Filters.Add<ValidatableAttribute>();
    public class ValidatableAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "x-action-intent";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            StringValues header = context.HttpContext.Request.Headers[HeaderName];
            string intent = header.FirstOrDefault();

            if (intent == "validate")
            {
                if (!context.ModelState.IsValid)
                {
                    // If the parameters that were send are not valid we will also
                    // in all cases short-circuit the pipeline.
                    context.Result = new BadRequestObjectResult(context.ModelState);
                }
                else
                {
                    // If the clients intent is 'validate' we will short-circuit
                    // the request pipeline and return HTTP OK.
                    context.Result = new NoContentResult();
                }
                return;
            }
        }
    }
}
