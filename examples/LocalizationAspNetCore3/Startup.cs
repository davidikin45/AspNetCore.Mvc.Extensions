using AspNetCore.Base.Localization;
using AspNetCore.Mvc.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using AspNetCore.Mvc.Extensions.AmbientRouteData;
using AspNetCore.Mvc.Extensions.FeatureFolders;
using System;

namespace LocalizationAspNetCore3
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
            });

            Action<FeatureFolderOptions> featureFoldersSetup = (options) =>
            {
                options.SharedViewFolders.Add("Bundles");
                options.SharedViewFolders.Add("Navigation");
                options.SharedViewFolders.Add("Footer");
                options.SharedViewFolders.Add("CookieConsent");
            };

            services.AddControllersWithViews(options =>
            {
                options.EnableEndpointRouting = true;

                if (AlwaysIncludeCultureInUrl)
                    options.AddCultureRouteConvention("cultureCheck");
                else
                    options.AddOptionalCultureRouteConvention("cultureCheck");

                //Middleware Pipeline - Wraps MVC
                options.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
             .AddFeatureFolders(featureFoldersSetup)
             .AddAreaFeatureFolders(featureFoldersSetup)
             .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
             .AddDataAnnotationsLocalization()
             //If EnableEndpointRouting is enabled (enabled by default from 2.2) ambient route data is required. 
             .AddAmbientRouteDataUrlHelperFactory(options =>
              {
                  options.AmbientRouteDataKeys.Add(new AmbientRouteData("area", false));
                  options.AmbientRouteDataKeys.Add(new AmbientRouteData("culture", true));
                  options.AmbientRouteDataKeys.Add(new AmbientRouteData("ui-culture", true));
              });

            services.AddRazorPages();

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<RequestLocalizationOptions> localizationOptions)
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();

                if (AlwaysIncludeCultureInUrl)
                {
                    endpoints.MapMiddlewareGet("{culture:cultureCheck}/{*path}", appBuilder =>
                    {
                       
                    });

                    //redirect culture-less routes
                    endpoints.MapGet("{*path}", ctx =>
                    {
                        var defaultCulture = localizationOptions.Value.DefaultRequestCulture.Culture.Name;

                        var cultureFeature = ctx.Features.Get<IRequestCultureFeature>();
                        var actualCulture = cultureFeature?.RequestCulture.Culture.Name;
                        var actualCultureLanguage = cultureFeature?.RequestCulture.Culture.TwoLetterISOLanguageName;

                        var path = ctx.GetRouteValue("path") ?? string.Empty;
                        var culturedPath = $"{ctx.Request.PathBase}/{actualCulture}/{path}{ctx.Request.QueryString.ToString()}";
                        ctx.Response.Redirect(culturedPath);
                        return Task.CompletedTask;
                    });
                }
            });
        }
    }
}
