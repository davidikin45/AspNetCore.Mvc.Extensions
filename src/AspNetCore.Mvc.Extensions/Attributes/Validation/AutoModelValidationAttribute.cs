using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;


namespace AspNetCore.Mvc.Extensions.Attributes.Validation
{
    public class AutoModelValidationAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Don't need this
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ViewResult()
                {
                    ViewData = ((Controller)context.Controller).ViewData,
                    TempData = ((Controller)context.Controller).TempData,
                    StatusCode = 400
                };
            }
        }
    }
}
