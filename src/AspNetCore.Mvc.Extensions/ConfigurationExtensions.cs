using AspNetCore.Base.ModelBinders;
using AspNetCore.Mvc.Extensions.AmbientRouteData;
using AspNetCore.Mvc.Extensions.Authentication;
using AspNetCore.Mvc.Extensions.Conventions.Display;
using AspNetCore.Mvc.Extensions.FeatureFolders;
using AspNetCore.Mvc.Extensions.FluentMetadata;
using AspNetCore.Mvc.Extensions.HostedServices;
using AspNetCore.Mvc.Extensions.Internal;
using AspNetCore.Mvc.Extensions.Localization;
using AspNetCore.Mvc.Extensions.NdjsonStream;
using AspNetCore.Mvc.Extensions.Providers;
using AspNetCore.Mvc.Extensions.Razor;
using AspNetCore.Mvc.Extensions.Reflection;
using AspNetCore.Mvc.Extensions.Routing.Constraints;
using AspNetCore.Mvc.Extensions.Services;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Swagger;
using AspNetCore.Mvc.Extensions.Validation;
using AspNetCore.Mvc.Extensions.VariableResourceRepresentation;
using Database.Initialization;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Initialization;
using Hangfire.MemoryStorage;
using Hangfire.Server;
using Hangfire.SQLite;
using Hangfire.SqlServer;
using Hangfire.States;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions
{
    public static class ConfigurationExtensions
    {
        #region Display Conventions
        public static IMvcBuilder AddAppendAsterixToRequiredFieldLabels(this IMvcBuilder builder)
        {
            var services = builder.Services;
            //Appends '*' to required fields
            services.AddTransient(sp => sp.GetService<IOptions<ConventionsHtmlGeneratorOptions>>().Value);
            services.AddSingleton<IHtmlGenerator, ConventionsHtmlGenerator>();

            return builder;
        }

        public static IMvcBuilder AddAppendAsterixToRequiredFieldLabels(this IMvcBuilder builder, Action<ConventionsHtmlGeneratorOptions> setupAction)
        {
            var services = builder.Services;

            //Appends '*' to required fields
            builder.AddAppendAsterixToRequiredFieldLabels();
            services.Configure(setupAction);

            return builder;
        }


        /// <summary>
        /// Adds MVC display conventions services to the application.
        /// </summary>
        public static IMvcBuilder AddMvcDisplayConventions(this IMvcBuilder builder, params IDisplayConventionFilter[] displayConventions)
        {
            var services = builder.Services;

            services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new DisplayConventionsMetadataProvider(displayConventions));
            });

            if (displayConventions.OfType<AppendAsterixToRequiredFieldLabels>().Any())
            {
                var addAsterix = displayConventions.OfType<AppendAsterixToRequiredFieldLabels>().FirstOrDefault().LimitConvention;
                if (addAsterix != null)
                {
                    builder.AddAppendAsterixToRequiredFieldLabels(options => options.AddAstertix = addAsterix);
                }
                else
                {
                    builder.AddAppendAsterixToRequiredFieldLabels();
                }
            }

            return builder;
        }
        #endregion

        #region Validation Conventions
        /// <summary>
        /// Adds MVC validation conventions services to the application.
        /// </summary>
        public static IMvcBuilder AddMvcValidationConventions(this IMvcBuilder builder, params IValidationConventionFilter[] validationConventions)
        {
            var services = builder.Services;

            services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new ValidationConventionsMetadataProvider(validationConventions));
            });

            return builder;
        }

        #endregion

        #region Display Attributes
        /// <summary>
        /// Adds MVC display attribute services to the application.
        /// </summary>
        public static IMvcBuilder AddMvcDisplayAttributes(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfigureOptions<MvcOptions>, AttributeMetadataProviderSetup>();

            return builder;
        }

        public class AttributeMetadataProviderSetup : IConfigureOptions<MvcOptions>
        {
            private readonly IServiceProvider _serviceProvider;

            public AttributeMetadataProviderSetup(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public void Configure(MvcOptions options)
            {
                options.ModelMetadataDetailsProviders.Add(new AttributeMetadataProvider(_serviceProvider));
            }
        }

        #endregion

        #region Client Side Validation Inheritance
        /// <summary>
        /// Adds MVC Inheritance Validation services to the application.
        /// </summary>
        public static IMvcBuilder AddMvcInheritanceValidationAttributeAdapterProvider(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.RemoveAll<IValidationAttributeAdapterProvider>();
            services.AddSingleton<IValidationAttributeAdapterProvider, InheritanceValidationAttributeAdapterProvider>();

            return builder;
        }

        #endregion

        #region Type Finder
        /// <summary>
        /// Adds the type finder service to the application.
        /// </summary>
        public static IServiceCollection AddTypeFinder(this IServiceCollection services)
        {
            services.AddTransient(sp => sp.GetService<IOptions<AssemblyProviderOptions>>().Value);

            services.AddSingleton<IAssemblyProvider, AssemblyProvider>();
            services.AddSingleton<ITypeFinder, TypeFinder>();

            return services;
        }
        /// <summary>
        /// Adds the type finder service to the application.
        /// </summary>
        public static IServiceCollection AddTypeFinder(this IServiceCollection services, Action<AssemblyProviderOptions> setupAction)
        {
            services.AddTypeFinder();
            services.Configure(setupAction);
            return services;
        }
        #endregion

        #region Fluent Metadata
        /// <summary>
        /// Adds the fluent metadata services.
        /// </summary>
        public static IServiceCollection AddFluentMetadata(this IServiceCollection services)
        {
            services.AddTypeFinder();
            services.AddSingleton<IMetadataConfiguratorProviderSingleton, MetadataConfiguratorProviderSingleton>();
            services.AddSingleton<IConfigureOptions<MvcOptions>, FluentMetadataConfigureMvcOptions>();

            return services;
        }
        public static IServiceCollection AddFluentMetadata(this IServiceCollection services, Action<AssemblyProviderOptions> setupAction)
        {
            services.AddFluentMetadata();
            services.Configure(setupAction);
            return services;
        }
        #endregion

        #region View Render
        /// <summary>
        /// Adds MVC IViewRenderService service to the application.
        /// </summary>
        public static IMvcBuilder AddMvcViewRenderer(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddHttpContextAccessor();
            services.AddSingleton<IViewRenderService, ViewRenderService>();

            return builder;
        }
        #endregion

        #region ND Json
        /// <summary>
        /// Adds the system.text.json ndjson services to the application.
        /// </summary>
        public static IMvcBuilder AddNdjsonStreamResult(this IMvcBuilder builder)
        {
            builder.Services.TryAddSingleton<INdjsonWriterFactory, NdjsonWriterFactory>();

            return builder;
        }

        /// <summary>
        /// Adds the newtonsoft ndjson services to the application.
        /// </summary>
        public static IMvcBuilder AddNewtonsoftNdjsonStreamResult(this IMvcBuilder builder)
        {
            builder.Services.TryAddSingleton<INdjsonWriterFactory, NewtonsoftNdjsonWriterFactory>();

            return builder;
        }
        #endregion

        #region Feature Service
        /// <summary>
        /// Adds MVC feature services to the application.
        /// </summary>
        public static IMvcBuilder AddMvcFeatureService(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<FeatureService>();

            return builder;
        }
        #endregion

        #region Bundle Config Service
        /// <summary>
        /// Adds bundleconfig service to the application.
        /// </summary>
        public static IServiceCollection AddBundleConfigService(this IServiceCollection services)
        {
            return services.AddSingleton<BundleConfigService>();
        }
        #endregion

        #region Json Navigation Service
        /// <summary>
        /// Adds MVC json navigation service to the application.
        /// </summary>
        public static IMvcBuilder AddMvcJsonNavigationService(this IMvcBuilder builder, Action<JsonNavigationServiceOptions> setupAction = null)
        {
            var services = builder.Services;

            services.AddSingleton<JsonNavigationService>();

            if (setupAction != null)
                services.Configure(setupAction);

            return builder;
        }
        #endregion region

        #region Ambient Route Data
        /// <summary>
        /// Adds ambient route data URL helper factory service to the application.
        /// </summary>
        public static IMvcBuilder AddAmbientRouteDataUrlHelperFactory(this IMvcBuilder builder, Action<AmbientRouteDataUrlHelperFactoryOptions> setupAction = null)
        {
            var services = builder.Services;

            if (setupAction != null)
                services.Configure(setupAction);

            services.Decorate<IUrlHelperFactory, AmbientRouteDataUrlHelperFactory>();

            return builder;
        }
        #endregion

        #region Model Binder and Input Formatters
        /// <summary>
        /// Adds the point model binder to the application.
        /// </summary>
        public static IMvcBuilder AddMvcPointModelBinder(this IMvcBuilder builder)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, PointMvcOptionsSetup>();
            return builder;
        }

        /// <summary>
        /// Adds the MVC raw string request body input formatter to the application.
        /// </summary>
        public static IMvcBuilder AddMvcRawStringRequestBodyInputFormatter(this IMvcBuilder builder)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, RawStringRequestBodyInputFormatterMvcOptionsSetup>();
            return builder;
        }

        /// <summary>
        /// Adds the MVC raw bytes request body input formatter to the application.
        /// </summary>
        public static IMvcBuilder AddMvcRawBytesRequestBodyInputFormatter(this IMvcBuilder builder)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, RawBytesRequestBodyInputFormatterMvcOptionsSetup>();
            return builder;
        }
        #endregion

        #region Feature Folders
        /// <summary>
        /// Adds feature folders to the application with format /{RootFeatureFolder}/{Controller}/{View}.cshtml, /{RootFeatureFolder}/{Controller}/Views/{View}.cshtml and /{RootFeatureFolder}/Shared/Views/{View}.cshtml
        /// </summary>
        public static IMvcBuilder AddFeatureFolders(this IMvcBuilder builder, Action<FeatureFolderOptions> setupAction = null)
        {
            var services = builder.Services;

            var options = new FeatureFolderOptions();
            if (setupAction != null)
                setupAction(options);

            //https://stackoverflow.com/questions/36747293/how-to-specify-the-view-location-in-asp-net-core-mvc-when-using-custom-locations
            services.Configure<RazorViewEngineOptions>(o =>
            {
                // {2} is area, {1} is controller,{0} is the action    
                //o.ViewLocationFormats.Clear();
                o.ViewLocationFormats.Add(options.RootFeatureFolder + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add(options.RootFeatureFolder + "/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add(options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in options.SharedViewFolders)
                {
                    o.ViewLocationFormats.Add(options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }
            });

            return builder;
        }

        /// <summary>
        /// Adds area feature folders to the application with format /Areas/{Area}/{RootFeatureFolder}/{Controller}/{View}.cshtml, /Areas/{Area}/{RootFeatureFolder}/{Controller}/Views/{View}.cshtml, /Areas/{Area}/{RootFeatureFolder}/Shared/Views/{View}.cshtml and /Areas/{Area}/Shared/Views/{View}.cshtml
        /// </summary>
        public static IMvcBuilder AddAreaFeatureFolders(this IMvcBuilder builder, Action<FeatureFolderOptions> setupAction = null)
        {
            var services = builder.Services;

            var options = new FeatureFolderOptions();
            if (setupAction != null)
                setupAction(options);

            services.Configure<RazorViewEngineOptions>(o =>
            {
                // {2} is area, {1} is controller,{0} is the action    
                //o.AreaViewLocationFormats.Clear();
                o.AreaViewLocationFormats.Add("/Areas/{2}" + options.RootFeatureFolder + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Areas/{2}" + options.RootFeatureFolder + "/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Areas/{2}" + options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in options.SharedViewFolders)
                {
                    o.AreaViewLocationFormats.Add("/Areas/{2}" + options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }

                o.AreaViewLocationFormats.Add("/Areas/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in options.SharedViewFolders)
                {
                    o.AreaViewLocationFormats.Add("/Areas/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }

                o.AreaViewLocationFormats.Add(options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in options.SharedViewFolders)
                {
                    o.AreaViewLocationFormats.Add(options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "{0}" + RazorViewEngine.ViewExtension);
                }
            });

            return builder;
        }

        #endregion

        #region Validation
        /// <summary>
        /// Disables the model validation.
        /// </summary>
        public static IMvcBuilder DisableModelValidation(this IMvcBuilder builder)
        {
            var validator = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(IObjectModelValidator));

            if (validator != null)
            {
                builder.Services.Remove(validator);
                builder.Services.Add(new ServiceDescriptor(typeof(IObjectModelValidator), _ => new NonValidatingValidator(), ServiceLifetime.Singleton));
            }

            return builder;
        }
        #endregion

        #region Authentication
        /// <summary>
        /// Uses the basic authentication.
        /// </summary>
        public static IApplicationBuilder UseBasicAuth(
           this IApplicationBuilder builder, string username, string password)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>("", username, password);
        }

        /// <summary>
        /// Adds basic authentication to the application.
        /// </summary>
        public static IServiceCollection AddBasicAuth(
           this IServiceCollection services)
        {
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationDefaults.AuthenticationScheme, null);
            return services;
        }

        /// <summary>
        /// Adds basic authentication to the application.
        /// </summary>
        public static IServiceCollection AddBasicAuth<TUser>(
           this IServiceCollection services)
            where TUser : IdentityUser
        {
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler<TUser>>(BasicAuthenticationDefaults.AuthenticationScheme, null);
            return services;
        }

        /// <summary>
        /// Users the must be authorized.
        /// </summary>
        public static IMvcBuilder UserMustBeAuthorized(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.Configure<MvcOptions>(options =>
            {
                //https://ondrejbalas.com/authorization-options-in-asp-net-core/
                //The authorization filter will be executed on any request that enters the MVC middleware and maps to a valid action.
                var globalPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                options.Filters.Add(new AuthorizeFilter(globalPolicy));
            });

            return builder;
        }
        #endregion

        #region Url Helper
        /// <summary>
        /// Adds the IUrlHelper service to the application. From .NET Core 2.2 onwards can use LinkGenerator service.
        /// </summary>
        public static IMvcBuilder AddUrlHelperService(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                           .ActionContext;
                return new UrlHelper(actionContext);
            });

            return builder;
        }
        #endregion

        #region Api
        /// <summary>
        /// Adds the API versioning services to the application.
        /// </summary>
        public static IMvcBuilder AddApiVersioning(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddVersionedApiExplorer(setupAction =>
            {
                setupAction.GroupNameFormat = "'v'VV";
                //Tells swagger to replace the version in the controller route
                setupAction.SubstituteApiVersionInUrl = true;
            });

            services.AddApiVersioning(option =>
            {
                //http://sundeepkamath.in/posts/rest-api-versioning-in-aspnet-core-part-1/
                //Query string parameter
                //URL path segment
                //HTTP header
                //Media type parameter

                option.ReportApiVersions = true;
                //Header then QueryString is consistent with how Accept header/FormatFilter works.
                option.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(), new MediaTypeApiVersionReader("v"), new HeaderApiVersionReader("api-version"), new QueryStringApiVersionReader("api-version", "v", "ver", "version"));
                //option.ApiVersionReader = new UrlSegmentApiVersionReader() /v{version:apiVersion}
                option.DefaultApiVersion = new ApiVersion(1, 0);
                option.AssumeDefaultVersionWhenUnspecified = true;

                //Add conventions
                //option.Conventions.Controller<>().HasApiVersion(new ApiVersion(1, 0));
            });

            return builder;
        }

        /// <summary>
        /// Adds swagger with API versioning.
        /// </summary>
        public static IMvcBuilder AddSwaggerWithApiVersioning(this IMvcBuilder builder, Action<SwaggerVersioningOptions> setupAction = null)
        {
            var services = builder.Services;

            services.AddSwaggerWithApiVersioning(setupAction);

            return builder;
        }

        /// <summary>
        /// Uses the swagger with API versioning.
        /// </summary>
        public static IApplicationBuilder UseSwaggerWithApiVersioning(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<SwaggerVersioningOptions>();

            var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            var swaggerEndpoints = new Dictionary<string, string>();
            foreach (var apiDescription in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                var swaggerEndpoint = $"/swagger/{apiDescription.GroupName}/swagger.json";
                swaggerEndpoints.Add(apiDescription.GroupName, swaggerEndpoint);
            }

            if (!string.IsNullOrWhiteSpace(options.UIUsername) && !string.IsNullOrWhiteSpace(options.UIPassword))
            {
                var routePrefix = options.SwaggerUIRoutePrefix;
                if (!string.IsNullOrEmpty(routePrefix) && !routePrefix.StartsWith("/"))
                    routePrefix = $"/{routePrefix}";
                if (!string.IsNullOrEmpty(routePrefix) && routePrefix.EndsWith("/"))
                    routePrefix = routePrefix.Substring(0, routePrefix.Length - 1);

                var swaggerSecured = swaggerEndpoints.Values.ToList().Concat(new[]{
                        $"{routePrefix}",
                        $"{routePrefix}/index.html" });

                app.UseWhen(context => swaggerSecured.Contains(context.Request.Path.ToString() == "/" ? "" : context.Request.Path.ToString(), StringComparer.OrdinalIgnoreCase),
                 appBranch =>
                 {
                     appBranch.UseBasicAuth(options.UIUsername, options.UIPassword);
                 });
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                //c.InjectStylesheet("/Assets/custum-ui.css");
                //c.IndexStream = () => GetType().Assembly.GetManifestResourceStream(".html");

                foreach (var swaggerEndpoint in swaggerEndpoints)
                {
                    c.SwaggerEndpoint(swaggerEndpoint.Value, options.ApiTitle + " " + swaggerEndpoint.Key.ToUpperInvariant());
                }

                c.RoutePrefix = options.SwaggerUIRoutePrefix;
                c.DocExpansion(DocExpansion.None);
                c.DefaultModelRendering(ModelRendering.Example);
                c.EnableDeepLinking();
                c.DisplayOperationId();
            });

            return app;
        }

        /// <summary>
        /// Configures the MVC variable resource representations for the application.
        /// </summary>
        public static IMvcBuilder ConfigureMvcVariableResourceRepresentations(this IMvcBuilder builder, Action<VariableResourceRepresentationOptions> setupAction = null)
        {
            //https://andrewlock.net/formatting-response-data-as-xml-or-json-based-on-the-url-in-asp-net-core/
            //Accept = Response MIME type client is able to understand.
            //Accept-Language = Response Language client is able to understand.
            //Accept-Encoding = Response Compression client is able to understand.

            var variableResourceRepresentationOptions = new VariableResourceRepresentationOptions();
            if (setupAction != null)
                setupAction(variableResourceRepresentationOptions);

            var services = builder.Services;

            services.Configure<MvcOptions>(options =>
            {
                //Prevents returning object representation in default format when request format isn't available
                options.ReturnHttpNotAcceptable = variableResourceRepresentationOptions.ReturnHttpNotAcceptable; //If Browser sends Accept not containing */* the server will try to find a formatter that can produce a response in one of the formats specified by the accept header.
                options.RespectBrowserAcceptHeader = variableResourceRepresentationOptions.RespectBrowserAcceptHeader; //If Browser sends Accept containing */* the server will ignore Accept header and use the first formatter that can format the object.

                //Variable resource representations
                //Use RequestHeaderMatchesMediaTypeAttribute to route for Accept header. Versioning should be done with media types not URI!
                var jsonInputFormatter = options.InputFormatters
                .OfType<JsonInputFormatter>().FirstOrDefault();
                if (jsonInputFormatter != null)
                {
                    foreach (var mediaType in variableResourceRepresentationOptions.JsonInputMediaTypes)
                    {
                        jsonInputFormatter.SupportedMediaTypes.Add(mediaType);
                    }
                }

                var jsonOutputFormatter = options.OutputFormatters
                   .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    foreach (var mediaType in variableResourceRepresentationOptions.JsonOutputMediaTypes)
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Add(mediaType);
                    }

                    if (variableResourceRepresentationOptions.RemoveTextJsonTextOutputFormatter)
                    {
                        // remove text/json as it isn't the approved media type
                        // for working with JSON at API level
                        if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                        {
                            jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                        }
                    }
                }

                foreach (var formatterMapping in variableResourceRepresentationOptions.FormatterMappings)
                {
                    options.FormatterMappings.SetMediaTypeMappingForFormat(formatterMapping.Key, formatterMapping.Value);
                }
            });

            return builder;
        }
        #endregion

        #region Hosted Services
        /// <summary>
        /// Adds the hosted service with cron job schedules or use [CronJob] to the application.
        /// </summary>
        public static IServiceCollection AddHostedServiceCronJob<TCronJob>(this IServiceCollection services, params string[] cronSchedules)
            where TCronJob : class, IHostedServiceCronJob
        {
            services.AddScoped<TCronJob>();

            return services.AddTransient<IHostedService>(sp =>
            {
                var logger = sp.GetService<ILogger<HostedServiceCron<TCronJob>>>();
                return new HostedServiceCron<TCronJob>(sp, logger, cronSchedules);
            });
        }

        /// <summary>
        /// Adds IBackgroundTaskQueue to the application.
        /// </summary>
        public static IServiceCollection AddHostedServiceBackgroundTaskQueue(this IServiceCollection services)
        {
            services.AddHostedService<QueuedHostedService>();
            return services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }
        #endregion

        #region Hangfire
        /// <summary>
        /// Adds the hangfire services to the application.
        /// </summary>
        public static IServiceCollection AddHangfire(this IServiceCollection services, string serverName, string connectionString = "", Action<JobStorageOptions> configJobStorage = null, Action <BackgroundJobServerOptions> configAction = null, IEnumerable<IBackgroundProcess> additionalProcesses = null)
        {
            services.AddHangfire(config =>
            {
                config
                 .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                 .UseSimpleAssemblyNameTypeSerializer()
                 .UseRecommendedSerializerSettings();

                config.UseFilter(new HangfireLoggerAttribute());
                config.UseFilter(new HangfirePreserveOriginalQueueAttribute());

                if(connectionString != null)
                {
                    //Initializes Hangfire Schema if PrepareSchemaIfNecessary = true
                    var storage = HangfireJobStorage.GetJobStorage(connectionString, configJobStorage).JobStorage;

                    config.UseStorage(storage);
                    //config.UseMemoryStorage();
                    //config.UseSqlServerStorage(connectionString);
                    //config.UseSQLiteStorage(connectionString);
                }
            });

            if(connectionString != null)
            {
                //Launches Server as IHostedService
                services.AddHangfireServer(serverName, configAction, additionalProcesses);
            }

            return services;
        }

        //IBackgroundJobClient and IRecurringJobManager will only work when storage setup via services.AddHangfire
        public static IServiceCollection AddHangfireServer(this IServiceCollection services, string serverName, Action<BackgroundJobServerOptions> configAction = null, IEnumerable<IBackgroundProcess> additionalProcesses = null, JobStorage storage = null)
        {
            return services.AddTransient<IHostedService, BackgroundJobServerHostedService>(provider =>
            {
                ThrowIfNotConfigured(provider);

                var options = new BackgroundJobServerOptions
                {
                    ServerName = serverName,
                    Queues = new[] { serverName, "default" }
                };

                if (configAction != null)
                    configAction(options);

                storage = storage ?? provider.GetService<JobStorage>() ?? JobStorage.Current;
                additionalProcesses = additionalProcesses ?? provider.GetServices<IBackgroundProcess>();

                options.Activator = options.Activator ?? provider.GetService<JobActivator>();
                options.FilterProvider = options.FilterProvider ?? provider.GetService<IJobFilterProvider>();
                options.TimeZoneResolver = options.TimeZoneResolver ?? provider.GetService<ITimeZoneResolver>();

                GetInternalServices(provider, out var factory, out var stateChanger, out var performer);

#pragma warning disable 618
                return new BackgroundJobServerHostedService(
#pragma warning restore 618
                    storage, options, additionalProcesses, factory, performer, stateChanger);
            });
        }

        public static IServiceCollection AddHangfireServerServices(this IServiceCollection services, Action<BackgroundJobServerOptions> configAction = null, JobStorage storage = null)
        {
            var options = new BackgroundJobServerOptions();
            if (configAction != null)
                configAction(options);

            services.AddSingleton<IBackgroundJobClient>(x =>
            {
                ThrowIfNotConfigured(x);

                if (GetInternalServices(x, out var factory, out var stateChanger, out _))
                {
                    return new BackgroundJobClient(storage ?? x.GetRequiredService<JobStorage>(), factory, stateChanger);
                }

                return new BackgroundJobClient(
                    storage ?? x.GetRequiredService<JobStorage>(),
                    options.FilterProvider ?? x.GetRequiredService<IJobFilterProvider>());
            });

            services.AddSingleton<IRecurringJobManager>(x =>
            {
                ThrowIfNotConfigured(x);

                if (GetInternalServices(x, out var factory, out _, out _))
                {
                    return new RecurringJobManager(
                        storage ?? x.GetRequiredService<JobStorage>(),
                        factory,
                         options.TimeZoneResolver ?? x.GetService<ITimeZoneResolver>());
                }

                return new RecurringJobManager(
                   storage ?? x.GetRequiredService<JobStorage>(),
                    options.FilterProvider ?? x.GetRequiredService<IJobFilterProvider>(),
                    options.TimeZoneResolver ?? x.GetService<ITimeZoneResolver>());
            });

            return services;
        }

        internal static void ThrowIfNotConfigured(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetService<IGlobalConfiguration>();
            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddHangfire' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }
        }

        internal static bool GetInternalServices(
           IServiceProvider provider,
           out IBackgroundJobFactory factory,
           out IBackgroundJobStateChanger stateChanger,
           out IBackgroundJobPerformer performer)
        {
            factory = provider.GetService<IBackgroundJobFactory>();
            performer = provider.GetService<IBackgroundJobPerformer>();
            stateChanger = provider.GetService<IBackgroundJobStateChanger>();

            if (factory != null && performer != null && stateChanger != null)
            {
                return true;
            }

            factory = null;
            performer = null;
            stateChanger = null;

            return false;
        }

        /// <summary>
        /// Exposes hangfire dashboard.
        /// </summary>
        public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder builder, string route = "/hangfire", Action<DashboardOptions> configAction = null, JobStorage storage = null)
        {
            var options = new DashboardOptions
            {
                //must be set otherwise only local access allowed
                //Authorization = new[] { new HangfireRoleAuthorizationfilter() },
                AppPath = route.Replace("/hangfire", "")
            };

            if (configAction != null)
                configAction(options);

            builder.UseHangfireDashboard(route, options, storage);

            return builder;
        }

        /// <summary>
        /// Starts the hangfire server. Better to use AddHangfireServer.
        /// </summary>
        public static IApplicationBuilder UseHangfireServer(this IApplicationBuilder builder, string serverName, IEnumerable<IBackgroundProcess> additionalProcesses = null, JobStorage storage = null)
        {
            //each microserver has its own queue. Queue by using the Queue attribute.
            //https://discuss.hangfire.io/t/one-queue-for-the-whole-farm-and-one-queue-by-server/490
            var options = new BackgroundJobServerOptions
            {
                ServerName = serverName,
                Queues = new[] { serverName, "default" }
            };

            //https://discuss.hangfire.io/t/one-queue-for-the-whole-farm-and-one-queue-by-server/490/3

            builder.UseHangfireServer(options, additionalProcesses, storage);
            return builder;
        }

        public static IServiceCollection AddHangfireJob<HangfireJob>(this IServiceCollection services)
            where HangfireJob : class
        {
            return services.AddTransient<HangfireJob>();
        }
        #endregion

        #region Localization
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
            routes.MapMiddlewareGet("{culture" + (!string.IsNullOrEmpty(cultureConstraintKey) ? $":{cultureConstraintKey}" : "") + "}/{*path}", appBuilder =>
            {

            });

            //redirect culture-less routes
            routes.MapGet("{*path}", (RequestDelegate)(ctx =>
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

        #endregion

        #region Azure Key Vault
        //https://joonasw.net/view/azure-ad-managed-service-identity
        //https://joonasw.net/view/aspnet-core-azure-keyvault-msi
        /// <summary>
        /// Uses the azure key vault. Looks for AppSettings.Json KeyVaultSettings > Name. Each environment should have a seperate vaultName. See https://joonasw.net/view/azure-ad-managed-service-identity for enabling MSI.
        /// </summary>
        public static IWebHostBuilder UseAzureKeyVault(this IWebHostBuilder webHostBuilder, string vaultName = null, bool useOnlyInProduction = true)
        {
            return webHostBuilder.ConfigureAppConfiguration((ctx, builder) =>
            {
                var config = builder.Build();

                if (vaultName == null)
                {
                    var keyVaultSettings = GetKeyVaultSettings(config);
                    if (keyVaultSettings != null)
                    {
                        vaultName = keyVaultSettings.Name;
                    }
                }

                //If used outside Azure, it will authenticate as the developer's user. It will try using Azure CLI 2.0 (install from here). The second option is AD Integrated Authentication.
                //After installing the CLI, remember to run az login, and login to your Azure account before running the app.Another important thing is that you need to also select the subscription where the Key Vault is.So if you have access to more than one subscription, also run az account set - s "My Azure Subscription name or id"
                //Then you need to make sure your user has access to a Key Vault(does not have to be the production vault...).
                if (!string.IsNullOrWhiteSpace(vaultName) && (!useOnlyInProduction || ctx.HostingEnvironment.IsProduction()))
                {
                    //Section--Name
                    var tokenProvider = new AzureServiceTokenProvider();
                    var kvClient = new KeyVaultClient((authority, resource, scope) => tokenProvider.KeyVaultTokenCallback(authority, resource, scope));
                    builder.AddAzureKeyVault($"https://{vaultName}.vault.azure.net", kvClient, new DefaultKeyVaultSecretManager());
                }
            });
        }

        private static KeyVaultSettings GetKeyVaultSettings(IConfigurationRoot config)
        {
            var sectionKey = "KeyVaultSettings";
            var hasKeyVaultSettings = config.GetChildren().Any(item => item.Key == sectionKey);

            if (!hasKeyVaultSettings)
                return null;

            var settingsSection = config.GetSection(sectionKey);
            return settingsSection.Get<KeyVaultSettings>();
        }
        #endregion
    }
}
