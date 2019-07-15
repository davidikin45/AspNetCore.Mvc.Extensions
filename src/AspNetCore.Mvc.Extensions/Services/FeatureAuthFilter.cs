using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Mvc.Extensions.Services
{
    //Add this to controller to enable and disable features
    //[TypeFilter(typeof(FeatureAuthFilter), Arguments = new object[] { "Loan" })]
    public class FeatureAuthFilter : IAuthorizationFilter
    {
        private FeatureService featureService;
        private string featureName;

        public FeatureAuthFilter(FeatureService featureService, string featureName)
        {
            this.featureService = featureService;
            this.featureName = featureName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!featureService.IsFeatureActive(featureName))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
