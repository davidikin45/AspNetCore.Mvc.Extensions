using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCore.Mvc.Extensions.Controllers.Mvc
{
    [Authorize()]
    public abstract class MvcControllerAuthorizeBase : MvcControllerBase
    {
        public MvcControllerAuthorizeBase()
        {

        }

        public MvcControllerAuthorizeBase(ControllerServicesContext context)
            :base(context)
        {
         
        }
    }
}
