using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Middleware;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Controllers.Mvc
{
    public abstract class MvcControllerAdminAuthorizeBase : MvcControllerAuthorizeBase
    {

        public MvcControllerAdminAuthorizeBase(ControllerServicesContext context)
             : base(context)
        {
        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("file-manager")]
        public ActionResult FileManager()
        {
            return View();
        }

        //https://stackoverflow.com/questions/565239/any-way-to-clear-flush-remove-outputcache/16038654
        [Route("clear-cache")]
        public virtual ActionResult ClearCache()
        {
            ResponseCachingCustomMiddleware.ClearResponseCache();
            return View();
        }
    }
}
