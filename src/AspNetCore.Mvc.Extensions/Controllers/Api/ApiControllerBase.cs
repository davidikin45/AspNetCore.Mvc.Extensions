using AspNetCore.Mvc.Extensions.ActionResults;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Validation;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    //C - Create - POST
    //R - Read - GET
    //U - Update - PUT
    //D - Delete - DELETE

    //If there is an attribute applied(via[HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
    //If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
    //Otherwise, the action supports the POST method.
    [Consumes("application/json", "application/xml")]
    [Produces("application/json", "application/xml")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        public IMapper Mapper { get; }
        public IEmailService EmailService { get; }
        public LinkGenerator LinkGenerator { get; }
        public AppSettings AppSettings { get; }

        public ApiControllerBase()
        {

        }

        public ApiControllerBase(ControllerServicesContext context)
        {
            Mapper = context.Mapper;
            EmailService = context.EmailService;
            LinkGenerator = context.LinkGenerator;
            AppSettings = context.AppSettings;
        }

        //https://docs.microsoft.com/en-us/aspnet/core/migration/claimsprincipal-current?view=aspnetcore-2.0
        public string Username
        {
            get
            {
                if (User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    return User.Identity.Name;
                }
                else
                {
                    return null;
                }
            }
        }

        public string UserId
        {
            get
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return null;
                }

                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }
        }

        protected IActionResult BulkTriggerActionResponse(IEnumerable<Result> results)
        {
            var webApiMessages = new List<ValidationProblemDetails>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                {
                    webApiMessages.Add(new ValidationProblemDetails() { Status = StatusCodes.Status200OK, Type = "about:blank" });
                }
                else
                {
                    webApiMessages.Add((ValidationProblemDetails)((ObjectResult)ValidationErrors(result)).Value);
                }
            }

            //For bulk return 200 regardless
            return Ok(webApiMessages);
        }

        protected List<ValidationProblemDetails> BulkCreateResponse(IEnumerable<Result> results)
        {
            var webApiMessages = new List<ValidationProblemDetails>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                {
                    webApiMessages.Add(new ValidationProblemDetails() { Status = StatusCodes.Status200OK, Type="about:blank" });
                }
                else
                {
                    webApiMessages.Add((ValidationProblemDetails)((ObjectResult)ValidationErrors(result)).Value);
                }
            }

            //For bulk return 200 regardless
            return webApiMessages;
        }

        protected List<ValidationProblemDetails> BulkUpdateResponse(IEnumerable<Result> results)
        {
            var webApiMessages = new List<ValidationProblemDetails>();

            foreach (var result in results)
            {
                if(result.IsSuccess)
                {
                    webApiMessages.Add(new ValidationProblemDetails() { Status = StatusCodes.Status200OK, Type = "about:blank" });
                }
                else
                {
                    webApiMessages.Add((ValidationProblemDetails)((ObjectResult)ValidationErrors(result)).Value);
                }
            }

            //For bulk return 200 regardless
            return webApiMessages;
        }

        protected List<ValidationProblemDetails> BulkDeleteResponse(IEnumerable<Result> results)
        {
            var webApiMessages = new List<ValidationProblemDetails>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                {
                    webApiMessages.Add(new ValidationProblemDetails() { Status = StatusCodes.Status200OK, Type = "about:blank" });
                }
                else
                {
                    webApiMessages.Add((ValidationProblemDetails)((ObjectResult)ValidationErrors(result)).Value);
                }
            }

            //For bulk return 200 regardless
            return webApiMessages;
        }

        protected CancellationToken ClientDisconnectedToken()
        {
            return this.HttpContext.RequestAborted;
        }

        protected virtual IActionResult Html(string html)
        {
            return new HtmlResult(html);
        }

        protected virtual IActionResult Forbidden()
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        protected virtual ActionResult Error(string errorMessage)
        {
            return BadRequest(errorMessage);
        }

        protected virtual ActionResult BadRequest(string errorMessage)
        {
            var problemDetails = MvcAsApi.Factories.ProblemDetailsFactory.GetProblemDetails(HttpContext, "Bad Request.", StatusCodes.Status400BadRequest, errorMessage);

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes =
                    {
                        "application/problem+json",
                        "application/problem+xml",
                    },
            };
        }

        protected ActionResult ValidationErrors(Result failure)
        {
            var newModelState = new ModelStateDictionary();
            switch (failure.ErrorType)
            {
                case ErrorType.ObjectValidationFailed:
                    newModelState.AddValidationErrors(failure.ObjectValidationErrors);
                    break;
                case ErrorType.ObjectDoesNotExist:
                    return NotFound();
                case ErrorType.ConcurrencyConflict:
                    newModelState.AddValidationErrors(failure.ObjectValidationErrors);
                    break;
                default:
                    //perhaps should be throwing so Startup returns a 500
                    //throw ex;
                    newModelState.AddModelError("", Messages.UnknownError);
                    break;
            }
            return ValidationErrors(newModelState);
        }

        protected ActionResult ValidationErrors()
        {
            return ValidationErrors(ModelState);
        }

        protected virtual ActionResult ValidationErrors(ModelStateDictionary modelState)
        {
            var problemDetails = MvcAsApi.Factories.ProblemDetailsFactory.GetValidationProblemDetails(HttpContext, modelState, StatusCodes.Status422UnprocessableEntity, true);

             return new ObjectResult(problemDetails)
             {
                 StatusCode = problemDetails.Status,
                 ContentTypes =
                    {
                        "application/problem+json",
                        "application/problem+xml",
                    },
             };
        }

        //Use factory for controller validationProblems
        public override ActionResult ValidationProblem(ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(new ActionContext(ControllerContext.HttpContext, ControllerContext.RouteData, ControllerContext.ActionDescriptor, modelStateDictionary));
        }

        protected IActionResult FromResult(Result result)
        {
            return result.IsSuccess ? Ok() : ValidationErrors(result);
        }
        protected IActionResult FromResult<T>(Result<T> result)
        {
            //ok(null) will return a 204
            return result.IsSuccess ? Ok(result.Value) : ValidationErrors(result);
        }
    }
}

