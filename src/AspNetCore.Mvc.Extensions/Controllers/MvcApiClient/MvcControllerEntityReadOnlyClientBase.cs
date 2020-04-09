using AspNetCore.Mvc.Extensions.Application;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Controllers.ApiClient;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System;

namespace AspNetCore.Mvc.Extensions.Controllers.MvcApiClient
{

    //Edit returns a view of the resource being edited, the Update updates the resource it self

    //C - Create - POST
    //R - Read - GET
    //U - Update - PUT
    //D - Delete - DELETE

    //If there is an attribute applied(via[HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
    //If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
    //Otherwise, the action supports the POST method.
    [AllowAnonymous]
    public abstract class MvcControllerEntityReadOnlyClientBase<TDto, IEntityService> : MvcControllerEntityReadOnlyClientAuthorizeBase<TDto, IEntityService>
        where TDto : class
        where IEntityService : IApiControllerEntityReadOnlyClient<TDto>
    {

        public MvcControllerEntityReadOnlyClientBase(ControllerServicesContext context, Boolean admin, IEntityService service)
        : base(context, admin, service)
        {

        }

    }
}

