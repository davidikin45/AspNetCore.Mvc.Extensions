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
    [AllowAnonymous]
    public abstract class ApiControllerEntityBase<TCreateDto, TReadDto, TUpdateDto, TDeleteDto, IEntityService> : ApiControllerEntityAuthorizeBase<TCreateDto, TReadDto, TUpdateDto, TDeleteDto, IEntityService>
        where TCreateDto : class
        where TReadDto : class
        where TUpdateDto : class
        where TDeleteDto : class
        where IEntityService : IApplicationServiceEntity<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>
    {   
        public ApiControllerEntityBase(ControllerServicesContext context, IEntityService service)
        : base(context, service)
        {
          
        }
    }
}

