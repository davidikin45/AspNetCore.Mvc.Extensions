using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Features
{
    public class RedirectDisabledFeatureToActionHandler : RedirectDisabledFeatureToActionResultHandler
    {

        public RedirectDisabledFeatureToActionHandler(string actionName = "Index", string controllerName = "Home", object routeValues = null)
            :base(new RedirectToActionResult(actionName, controllerName, routeValues))
        {

        }


    }
}