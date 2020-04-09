using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Controllers.Angular
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class MvcControllerAngularServerTemplateBase : Controller
    {
        [Route("{feature}/Template/{name}")]
        public PartialViewResult Render(string feature, string name)
        {
            return PartialView(string.Format("~/wwwroot/js/app/{0}/templates/{1}", feature, name));
        }
    }
}
