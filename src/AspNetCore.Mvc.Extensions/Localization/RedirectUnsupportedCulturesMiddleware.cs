using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Localization
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRedirectUnsupportedCultures(this IApplicationBuilder app, Action<RedirectUnsupportedCultureOptions> configureOptions = null)
        {
            var options = new RedirectUnsupportedCultureOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }

            return app.UseRedirectUnsupportedCultures(options);
        }

        public static IApplicationBuilder UseRedirectUnsupportedCultures(this IApplicationBuilder app, RedirectUnsupportedCultureOptions options)
        {
            return app.UseMiddleware<RedirectUnsupportedCulturesMiddleware>(options);
        }
    }

    public class RedirectUnsupportedCulturesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _routeDataStringKey;
        private readonly RedirectUnsupportedCultureOptions _options; 

        public RedirectUnsupportedCulturesMiddleware(
            RequestDelegate next,
            RequestLocalizationOptions options,
            RedirectUnsupportedCultureOptions redirectUnsupportedCultureOptions)
        {
            _next = next;
            var provider = options.RequestCultureProviders
                .Select(x => x as RouteDataRequestCultureProvider)
                .Where(x => x != null)
                .FirstOrDefault();
            _routeDataStringKey = provider.RouteDataStringKey;
            _options = redirectUnsupportedCultureOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestedCulture = context.GetRouteValue(_routeDataStringKey)?.ToString();
            var cultureFeature = context.Features.Get<IRequestCultureFeature>();

            var actualCulture = cultureFeature?.RequestCulture.Culture.Name;

            if ((_options.RedirectCultureless && string.IsNullOrEmpty(requestedCulture)) || (_options.RedirectUnspportedCultures && !string.IsNullOrEmpty(requestedCulture) && !string.Equals(requestedCulture, actualCulture, StringComparison.OrdinalIgnoreCase)))
            {
                var newCulturedPath = GetNewPath(context, actualCulture);
                context.Response.Redirect(newCulturedPath);
                return;
            }

            await _next.Invoke(context);
        }

        private string GetNewPath(HttpContext context, string newCulture)
        {
            var routeData = context.GetRouteData();
            var router = routeData.Routers[0];
            var virtualPathContext = new VirtualPathContext(
                context,
                routeData.Values,
                new RouteValueDictionary { { _routeDataStringKey, newCulture } });

            return router.GetVirtualPath(virtualPathContext).VirtualPath;
        }
    }

    public class RedirectUnsupportedCultureOptions
    {
        public bool RedirectCultureless { get; set; } = false;
        public bool RedirectUnspportedCultures { get; set; } = true;
    }
}
