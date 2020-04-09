using AspNetCore.Mvc.Extensions.Application;
using AspNetCore.Mvc.Extensions.Context;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    //Edit returns a view of the resource being edited, the Update updates the resource it self

    //C - Create - POST
    //R - Read - GET
    //U - Update - PUT
    //D - Delete - DELETE

    //If there is an attribute applied(via[HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
    //If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
    //Otherwise, the action supports the POST method.

    //AuthorizeAttributes are AND not OR.
    //AllowAnonymous at Controller or Action level always takes priority!
    [AllowAnonymous] // 40
    public abstract class ApiControllerEntityReadOnlyBase<TDto, IEntityService> : ApiControllerEntityReadOnlyAuthorizeBase<TDto, IEntityService>
        where TDto : class
        where IEntityService : IApplicationServiceEntityReadOnly<TDto>
    {   
        public ApiControllerEntityReadOnlyBase(ControllerServicesContext context, IEntityService service)
        : base(context, service)
        {
 
        }
    }
}

