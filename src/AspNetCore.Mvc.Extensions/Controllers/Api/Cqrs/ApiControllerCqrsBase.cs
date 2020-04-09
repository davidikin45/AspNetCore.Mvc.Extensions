using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Cqrs;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    [AllowAnonymous]
    public abstract class ApiControllerCqrsBase : ApiControllerCqrsAuthorizeBase
    {

        public ApiControllerCqrsBase(ControllerServicesContext context, ICqrsMediator cqrsMediator)
        : base(context, cqrsMediator)
        {
           
        }
    }
}

