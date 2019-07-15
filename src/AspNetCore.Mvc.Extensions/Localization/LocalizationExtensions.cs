using AspNetCore.Mvc.Extensions.Routing.Constraints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Base.Localization
{
    //https://stackoverflow.com/questions/40828570/asp-net-core-model-binding-error-messages-localization
    public static class LocalizationExtensions
    {

        //"LocalizationSettings": {
        //  "DefaultCulture": "en-AU",
        //  "SupportedUICultures": [ "en" ],
        //  "SupportAllLanguagesFormatting": false,
        //  "SupportAllCountriesFormatting": false,
        //  "SupportUICultureFormatting": true,
        //  "SupportDefaultCultureLanguageFormatting": true,
        //  "AlwaysIncludeCultureInUrl": true
        //},

        //Middleware Pipeline - Wraps MVC
        //options.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));
        public static IServiceCollection AddRequestLocalizationOptions(this IServiceCollection services,
            string defaultCulture,
            bool supportAllCountryFormatting,
            bool supportAllLanguagesFormatting,
            bool supportUICultureFormatting,
            bool allowDefaultCultureLanguage,
            params string[] supportedUICultures)
        {
            //https://andrewlock.net/adding-localisation-to-an-asp-net-core-application/
            //Default culture should be set to where the majority of traffic comes from.
            //If the client sends through "en" and the default culture is "en-AU". Instead of falling back to "en" it will fall back to "en-AU".
            var defaultLanguage = defaultCulture.Split('-')[0];

            //Support all formats for numbers, dates, etc.
            var formatCulturesList = new List<string>() { };

            if (supportAllLanguagesFormatting || supportAllLanguagesFormatting)
            {
                var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(language => language.Name != "").ToList();

                //Languages = en
                foreach (CultureInfo language in languages)
                {
                    if (supportAllLanguagesFormatting)
                    {
                        if (!formatCulturesList.Contains(language.Name) && (allowDefaultCultureLanguage || language.Name != defaultLanguage))
                        {
                            formatCulturesList.Add(language.Name);
                        }
                    }

                    //Countries = en-US
                    if (supportAllCountryFormatting)
                    {
                        var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(country => country.Parent.Equals(language)).ToList();
                        foreach (CultureInfo country in countries)
                        {
                            if (!formatCulturesList.Contains(country.Name))
                            {
                                formatCulturesList.Add(country.Name);
                            }
                        }
                    }
                }
            }

            if (supportUICultureFormatting)
            {
                foreach (var supportedUICulture in supportedUICultures)
                {
                    var countryOrLanguage = CultureInfo.GetCultureInfo(supportedUICulture);

                    if (!formatCulturesList.Contains(countryOrLanguage.Name) && (allowDefaultCultureLanguage || countryOrLanguage.Name != defaultLanguage))
                    {
                        formatCulturesList.Add(countryOrLanguage.Name);
                    }

                    var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(x => x.Parent.Equals(countryOrLanguage)).ToList();

                    foreach (CultureInfo country in countries)
                    {
                        if (!formatCulturesList.Contains(country.Name))
                        {
                            formatCulturesList.Add(country.Name);
                        }
                    }
                }
            }

            var supportedFormatCultures = formatCulturesList.Select(x => new CultureInfo(x)).ToArray();

            var supportedUICultureInfoList = new List<CultureInfo>() { };

            foreach (var supportedUICulture in supportedUICultures)
            {
                supportedUICultureInfoList.Add(new CultureInfo(supportedUICulture));
            }

            var defaultUICulture = supportedUICultures[0];

            //Default culture providers
            //1. Query string
            //2. Cookie
            //3. Accept-Language header

            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.2
            //https://andrewlock.net/url-culture-provider-using-middleware-as-mvc-filter-in-asp-net-core-1-1-0/
            //https://andrewlock.net/applying-the-routedatarequest-cultureprovider-globally-with-middleware-as-filters/
            //https://gunnarpeipman.com/aspnet/aspnet-core-simple-localization/
            //http://sikorsky.pro/en/blog/aspnet-core-culture-route-parameter

            //Route("{culture}/{ui-culture}/[controller]")]
            //[Route("{culture}/[controller]")]

            //options.RequestCultureProviders.Insert(0, routeDataRequestProvider);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: defaultCulture, uiCulture: defaultUICulture);
                // Formatting numbers, dates, etc.
                options.SupportedCultures = supportedFormatCultures;
                // UI strings that we have localized.
                options.SupportedUICultures = supportedUICultureInfoList;
                options.RequestCultureProviders = new List<IRequestCultureProvider>()
                {
                     new RouteDataRequestCultureProvider() { Options = options, RouteDataStringKey = "culture", UIRouteDataStringKey = "ui-culture" },
                     new QueryStringRequestCultureProvider() { QueryStringKey = "culture", UIQueryStringKey = "ui-culture" },
                     new CookieRequestCultureProvider(),
                     new AcceptLanguageHeaderRequestCultureProvider(),
                };
            });

            services.AddSingleton(sp => sp.GetService<IOptions<RequestLocalizationOptions>>().Value);

            return services;
        }

        public static IServiceCollection ConfigureRedirectUnsupportedCultureOptions(this IServiceCollection services, Action<RedirectUnsupportedCultureOptions> configureOptions)
        {
            return services.Configure(configureOptions);
        }

        public static MvcOptions AddOptionalCultureRouteConvention(this MvcOptions options, string cultureConstraintKey = "cultureCheck")
        {
            options.Conventions.Insert(0, new LocalizationConvention(true, cultureConstraintKey));

            return options;
        }

        public static MvcOptions AddCultureRouteConvention(this MvcOptions options, string cultureConstraintKey = "cultureCheck")
        {
            options.Conventions.Insert(0, new LocalizationConvention(false, cultureConstraintKey));

            return options;
        }

        public static IServiceCollection AddCultureRouteConstraint(this IServiceCollection services, string cultureConstraintKey = "cultureCheck")
        {
            return services.Configure<RouteOptions>(options =>
             {
                 options.ConstraintMap.Add(cultureConstraintKey, typeof(CultureConstraint));
             });
        }

        //404 cultureless. Used with AddCultureRouteConvention.
        public static IRouteBuilder RedirectCulturelessToDefaultCulture(this IRouteBuilder routes, RequestLocalizationOptions localizationOptions, string cultureConstraintKey = "cultureCheck")
        {
            //any route 1.has a culture; and 2.does not match the previous global route url will return a 404.
            routes.MapRoute("{culture" + (!string.IsNullOrEmpty(cultureConstraintKey)? $":{cultureConstraintKey}" : "") +"}/{*path}", ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            });

            //redirect culture-less routes
            routes.MapRoute("{*path}", (RequestDelegate)(ctx =>
            {
                var defaultCulture = localizationOptions.DefaultRequestCulture.Culture.Name;

                var cultureFeature = ctx.Features.Get<IRequestCultureFeature>();
                var actualCulture = cultureFeature?.RequestCulture.Culture.Name;
                var actualCultureLanguage = cultureFeature?.RequestCulture.Culture.TwoLetterISOLanguageName;

                var path = ctx.GetRouteValue("path") ?? string.Empty;
                var culturedPath = $"{ctx.Request.PathBase}/{actualCulture}/{path}{ctx.Request.QueryString.ToString()}";
                ctx.Response.Redirect(culturedPath);
                return Task.CompletedTask;
            }));

            return routes;
        }
    }
}
