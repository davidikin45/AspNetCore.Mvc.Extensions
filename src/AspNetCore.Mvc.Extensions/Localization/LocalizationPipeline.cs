using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.Base.Localization
{
    //Middleware can only use data that has been added by preceding components in the pipeline, but we need access to routing information(the RouteData segments). 
    //Routing doesn't happen till the MVC middleware runs, which we need to run to extract the RouteData segments from the url. 
    //Therefore, we need request localisation to happen after action selection, but before the action executes; in other words, in the MVC filter pipeline.

    public class LocalizationPipeline
    {
        public void Configure(IApplicationBuilder app, IOptions<RequestLocalizationOptions> options, IOptions<RedirectUnsupportedCultureOptions> redirectUnsupportedCultureOptions)
        {
            app.UseRequestLocalization(options.Value);
            app.UseRedirectUnsupportedCultures(redirectUnsupportedCultureOptions.Value);
        }
    }
}
