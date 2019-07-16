using AspNetCore.Base.Localization;
using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.AmbientRouteData;
using AspNetCore.Mvc.Extensions.FeatureFolders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Localization
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public bool AlwaysIncludeCultureInUrl { get; set; } = true;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            Action<FeatureFolderOptions> featureFoldersSetup = (options) =>
            {
                options.SharedViewFolders.Add("Bundles");
                options.SharedViewFolders.Add("Navigation");
                options.SharedViewFolders.Add("Footer");
                options.SharedViewFolders.Add("CookieConsent");
            };

            services.AddMvc(options =>
            {
                //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
                //https://github.com/aspnet/AspNetCore/blob/1c126ab773059d6a5899fc29547cb86ed49c46bf/src/Http/Routing/src/Template/TemplateBinder.cs
                //EnableEndpointRouting = false Ambient route values always reused.
                //EnableEndpointRouting = true. Ambient route values only reused if generating link for same controller/action. 

                options.EnableEndpointRouting = true;

                if (AlwaysIncludeCultureInUrl)
                    options.AddCultureRouteConvention("cultureCheck");
                else
                    options.AddOptionalCultureRouteConvention("cultureCheck");

                //Middleware Pipeline - Wraps MVC
                options.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
             .AddFeatureFolders(featureFoldersSetup)
             .AddAreaFeatureFolders(featureFoldersSetup)
             .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
             .AddDataAnnotationsLocalization()
             //If EnableEndpointRouting is enabled (enabled by default from 2.2) ambient route data is required. 
             .AddAmbientRouteDataUrlHelperFactory(options => {
                 options.AmbientRouteDataKeys.Add(new AmbientRouteData("area", false));
                 options.AmbientRouteDataKeys.Add(new AmbientRouteData("culture", true));
                 options.AmbientRouteDataKeys.Add(new AmbientRouteData("ui-culture", true));
             });

            services.AddCultureRouteConstraint("cultureCheck");

            services.AddRequestLocalizationOptions(
               defaultCulture: "en-AU",
               supportAllCountryFormatting: false,
               supportAllLanguagesFormatting: false,
               supportUICultureFormatting: true,
               allowDefaultCultureLanguage: true,
               supportedUICultures: "en");

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<RequestLocalizationOptions> localizationOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRequestLocalization(localizationOptions.Value);

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {


            });

            var routeBuilder = new RouteBuilder(app);

            if (AlwaysIncludeCultureInUrl)
            {
                routeBuilder.RedirectCulturelessToDefaultCulture(localizationOptions.Value);
            }

            app.UseRouter(routeBuilder.Build());
        }
    }
}
