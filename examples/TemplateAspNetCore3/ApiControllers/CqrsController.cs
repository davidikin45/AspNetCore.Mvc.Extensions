using AspNetCore.Mvc.Extensions.Controllers.Api;
using AspNetCore.Mvc.Extensions.Cqrs;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace TemplateAspNetCore3.ApiControllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/cqrs")]
    public class CqrsController : ApiControllerCqrsBase
    {
        public CqrsController(ICqrsMediator cqrsMediator, IMapper mapper, IEmailService emailService, LinkGenerator linkGenerator, AppSettings appSettings)
            : base(cqrsMediator, mapper, emailService, linkGenerator, appSettings)
        {

        }
    }
}
