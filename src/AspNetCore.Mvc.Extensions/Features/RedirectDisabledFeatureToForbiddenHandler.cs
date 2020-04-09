using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Features
{
    public class RedirectDisabledFeatureToForbiddenHandler : RedirectDisabledFeatureToActionResultHandler
    {
        public RedirectDisabledFeatureToForbiddenHandler()
            :base(new ForbidResult())
        {

        }
    }
}