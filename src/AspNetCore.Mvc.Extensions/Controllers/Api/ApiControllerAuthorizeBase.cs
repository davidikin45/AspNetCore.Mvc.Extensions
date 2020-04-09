using AspNetCore.Mvc.Extensions.Authentication;
using AspNetCore.Mvc.Extensions.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    //C - Create - POST
    //R - Read - GET
    //U - Update - PUT
    //D - Delete - DELETE

    //If there is an attribute applied(via[HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
    //If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
    //Otherwise, the action supports the POST method.

    //AuthorizeAttributes are AND not OR.
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme+ "," + BasicAuthenticationDefaults.AuthenticationScheme, Roles = "admin")] // 40
    public abstract class ApiControllerAuthorizeBase : ApiControllerBase
    {
        public ApiControllerAuthorizeBase(ControllerServicesContext context)
            :base(context)
        {
           
        }
    }
}

