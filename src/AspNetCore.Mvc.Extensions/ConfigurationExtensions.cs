using AspNetCore.Mvc.Extensions.Authentication;
using AspNetCore.Mvc.Extensions.Conventions.Display;
using AspNetCore.Mvc.Extensions.Data.Configuration;
using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.DependencyInjection;
using AspNetCore.Mvc.Extensions.FeatureFolders;
using AspNetCore.Mvc.Extensions.FluentMetadata;
using AspNetCore.Mvc.Extensions.HealthChecks;
using AspNetCore.Mvc.Extensions.HealthChecks.File;
using AspNetCore.Mvc.Extensions.HealthChecks.Memory;
using AspNetCore.Mvc.Extensions.HealthChecks.Ping;
using AspNetCore.Mvc.Extensions.HostedServices;
using AspNetCore.Mvc.Extensions.HostedServices.FileProcessing;
using AspNetCore.Mvc.Extensions.Logging;
using AspNetCore.Mvc.Extensions.Mapping;
using AspNetCore.Mvc.Extensions.ModelBinders;
using AspNetCore.Mvc.Extensions.NdjsonStream;
using AspNetCore.Mvc.Extensions.Plugins;
using AspNetCore.Mvc.Extensions.Providers;
using AspNetCore.Mvc.Extensions.Razor;
using AspNetCore.Mvc.Extensions.Reflection;
using AspNetCore.Mvc.Extensions.Security;
using AspNetCore.Mvc.Extensions.Services;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.SignalR;
using AspNetCore.Mvc.Extensions.StartupTasks;
using AspNetCore.Mvc.Extensions.Swagger;
using AspNetCore.Mvc.Extensions.Users;
using AspNetCore.Mvc.Extensions.Validation;
using AspNetCore.Mvc.Extensions.Validation.Settings;
using AspNetCore.Mvc.Extensions.VariableResourceRepresentation;
using AspNetCore.Mvc.UrlLocalization;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.QueryableExtensions;
using Database.Initialization;
using EntityFrameworkCore.Initialization.NoSql;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using GraphQL.Types;
using JpProject.AspNetCore.PasswordHasher.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniProfiler.Initialization;
using Nest;
using NetTopologySuite.Geometries;
using Scrutor;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using StackExchange.Profiling.Storage;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace AspNetCore.Mvc.Extensions
{
    public static class ConfigurationExtensions
    {
        #region ApplicationParts - Controllers, Views, ViewComponents, TagHelpers

        public static IWebHostBuilder LogApplicationParts(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((services) =>
            {
                services.LogApplicationParts();
            });
        }

        //https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-3.1
        //https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts
        public static IServiceCollection LogApplicationParts(this IServiceCollection services)
        {
            return services.AddHostedService<ApplicationPartsLogger>();
        }
        #endregion

        #region Application Settings
        /// <summary>
        /// Validate settings on App Startup
        /// </summary>
        public static IServiceCollection UseStartupSettingsValidation(this IServiceCollection services)
        {
            //While IStartupFilters can be used to add middleware to the pipeline, they don't have to. Instead, they can simply be used to run some code when the app starts up, after service configuration has happened, but before the app starts handling requests.
            services.AddTransient<IStartupFilter, SettingsValidationStartupFilter>();

            return services;
        }

        /// <summary>
        /// Configures the settings.
        /// </summary>
        public static IServiceCollection ConfigureSettings<TOptions>(this IServiceCollection services, IConfiguration config) where TOptions : class, new()
        {
            // Bind the configuration using IOptions
            services.Configure<TOptions>(config);

            //services.Configure<TOptions>(o => { });
            //services.AddOptions<TOptions>().Configure(o => { });

            // Explicitly register the settings object so IOptions not required (optional)
            //Singleton
            //services.AddTransient(sp => sp.GetService<IOptions<TOptions>>().Value);

            //Scoped - Settings are rebound per request
            services.AddTransient(sp => sp.GetService<IOptionsSnapshot<TOptions>>().Value);

            if (typeof(IValidateSettings).IsAssignableFrom(typeof(TOptions)))
            {
                // Register as an IValidateSettings
                services.AddTransient<IValidateSettings>(sp => (IValidateSettings)sp.GetService<IOptions<TOptions>>().Value);
            }

            return services;
        }


        /// <summary>
        /// Configures the settings.
        /// </summary>
        public static IServiceCollection ConfigureSettings<TOptions>(this IServiceCollection services, Action<TOptions> configureOptions) where TOptions : class, new()
        {
            // Bind the configuration using IOptions
            services.Configure<TOptions>(configureOptions);

            // Explicitly register the settings object so IOptions not required (optional)
            services.AddTransient(sp => sp.GetService<IOptions<TOptions>>().Value);

            if (typeof(IValidateSettings).IsAssignableFrom(typeof(TOptions)))
            {
                // Register as an IValidateSettings
                services.AddTransient<IValidateSettings>(sp => (IValidateSettings)sp.GetService<IOptions<TOptions>>().Value);
            }

            return services;
        }
        #endregion

        #region Logging
        //https://www.humankode.com/asp-net-core/logging-with-elasticsearch-kibana-asp-net-core-and-docker
        public static LoggerConfiguration AddElasticSearchLogging(this LoggerConfiguration loggerConfiguration, string elasticUri)
        {
            if (!string.IsNullOrWhiteSpace(elasticUri))
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    AutoRegisterTemplate = true,
                });
            }

            return loggerConfiguration;
        }

        public static LoggerConfiguration AddElasticSearchLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var elasticSettingsSection = configuration.GetChildren().FirstOrDefault(item => item.Key == "ElasticSettings");
            if (elasticSettingsSection != null)
            {
                var log = elasticSettingsSection.GetValue<bool>("Log", false);
                var uri = elasticSettingsSection.GetValue<string>("Uri", "");

                if (log && string.IsNullOrWhiteSpace(uri))
                {
                    loggerConfiguration.AddElasticSearchLogging(uri);
                }
            }

            return loggerConfiguration;
        }

        public static LoggerConfiguration AddSeqLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var seqSettingsSection = configuration.GetChildren().FirstOrDefault(item => item.Key == "SeqSettings");
            if (seqSettingsSection != null)
            {
                var log = seqSettingsSection.GetValue<bool>("Log", false);
                var uri = seqSettingsSection.GetValue<string>("Uri", "");

                if (log && string.IsNullOrWhiteSpace(uri))
                {
                    loggerConfiguration.AddSeqLogging(uri);
                }
            }

            return loggerConfiguration;
        }

        public static LoggerConfiguration AddSeqLogging(this LoggerConfiguration loggerConfiguration, string seekUri)
        {
            if (!string.IsNullOrWhiteSpace(seekUri))
            {
                loggerConfiguration.WriteTo.Seq(seekUri);
            }

            return loggerConfiguration;
        }
        #endregion

        #region Display Conventions
        public static IMvcBuilder AddAppendAsterixToRequiredFieldLabels(this IMvcBuilder builder)
        {
            var services = builder.Services;
            //Appends '*' to required fields
            services.AddTransient(sp => sp.GetService<IOptions<ConventionsHtmlGeneratorOptions>>().Value);

            services.Decorate<IHtmlGenerator, ConventionsHtmlGenerator>();

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

        #region Suppress Point Child Validation
        /// <summary>
        /// Suppresses Point child validation.
        /// </summary>
        public static IMvcBuilder SuppressMvcPointChildValidation(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfigureOptions<MvcOptions>, SuppressPointChildValidationSetup>();

            return builder;
        }
        public class SuppressPointChildValidationSetup : IConfigureOptions<MvcOptions>
        {
            private readonly IServiceProvider _serviceProvider;

            public SuppressPointChildValidationSetup(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public void Configure(MvcOptions options)
            {
                options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Point)));
            }
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
        public static IMvcBuilder AddMvcViewRendererAndPdfGenerator(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IViewRenderService, ViewRenderService>();
            services.TryAddSingleton<IGeneratePdfService, GeneratePdfService>();

            services.AddSingleton<UpdateableFileProvider>();

            services.AddSingleton<IConfigureOptions<MvcRazorRuntimeCompilationOptions>, UpdateableFileProviderMvcRazorRuntimeCompilationOptionsSetup>();
            //.NET Core 2.2 services.AddSingleton<IConfigureOptions<RazorViewEngineOptions>, UpdateableFileProviderRazorViewEngineOptionssSetup>();

            return builder;
        }

        public static IMvcBuilder AddMvcViewRendererAndPdfGenerator(this IMvcBuilder builder, Action<GeneratePdfOptions> setupAction)
        {
            var services = builder.Services;

            services.Configure(setupAction);
            builder.AddMvcViewRendererAndPdfGenerator();

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
        public static IWebHostBuilder LogViewLocations(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((services) =>
            {
                services.LogViewLocations();
            });
        }

        public static IServiceCollection LogViewLocations(this IServiceCollection services)
        {
            return services.AddHostedService<ViewLocationsLogger>();
        }

        /// <summary>
        /// Adds feature folders to the application with format /{RootFeatureFolder}/{Controller}/{View}.cshtml, /{RootFeatureFolder}/{Controller}/Views/{View}.cshtml and /{RootFeatureFolder}/Shared/Views/{View}.cshtml
        /// </summary>
        public static IMvcBuilder AddFeatureFolders(this IMvcBuilder builder, Action<FeatureFolderOptions> setupAction = null)
        {
            var services = builder.Services;

            services.AddOptions();
            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton<IConfigureOptions<RazorViewEngineOptions>, FeatureFoldersSetup>();

            return builder;
        }

        //https://stackoverflow.com/questions/36747293/how-to-specify-the-view-location-in-asp-net-core-mvc-when-using-custom-locations
        public class FeatureFoldersSetup : IConfigureOptions<RazorViewEngineOptions>
        {
            private readonly ILogger<FeatureFoldersSetup> _logger;
            private readonly FeatureFolderOptions _options;

            public FeatureFoldersSetup(ILogger<FeatureFoldersSetup> logger, IOptions<FeatureFolderOptions> options)
            {
                _logger = logger;
                _options = options.Value;
            }

            public void Configure(RazorViewEngineOptions o)
            {
                // {2} is area, {1} is controller,{0} is the action    
                //o.ViewLocationFormats.Clear();
                o.ViewLocationFormats.Add(_options.RootFeatureFolder + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add(_options.RootFeatureFolder + "/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);
                o.PageViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in _options.SharedViewFolders)
                {
                    o.ViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                    o.PageViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }
            }
        }

        /// <summary>
        /// Adds area feature folders to the application with format /Areas/{Area}/{RootFeatureFolder}/{Controller}/{View}.cshtml, /Areas/{Area}/{RootFeatureFolder}/{Controller}/Views/{View}.cshtml, /Areas/{Area}/{RootFeatureFolder}/Shared/Views/{View}.cshtml and /Areas/{Area}/Shared/Views/{View}.cshtml
        /// </summary>
        public static IMvcBuilder AddAreaFeatureFolders(this IMvcBuilder builder, Action<AreaFeatureFolderOptions> setupAction = null)
        {
            var services = builder.Services;

            services.AddOptions();
            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton<IConfigureOptions<RazorViewEngineOptions>, AreaFeatureFoldersSetup>();

            return builder;
        }

        public class AreaFeatureFoldersSetup : IConfigureOptions<RazorViewEngineOptions>
        {
            private readonly ILogger<AreaFeatureFoldersSetup> _logger;
            private readonly AreaFeatureFolderOptions _options;

            public AreaFeatureFoldersSetup(ILogger<AreaFeatureFoldersSetup> logger, IOptions<AreaFeatureFolderOptions> options)
            {
                _logger = logger;
                _options = options.Value;
            }

            public void Configure(RazorViewEngineOptions o)
            {
                // {2} is area, {1} is controller,{0} is the action    
                //o.AreaViewLocationFormats.Clear();
                o.AreaViewLocationFormats.Add("/Areas/{2}" + _options.RootFeatureFolder + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Areas/{2}" + _options.RootFeatureFolder + "/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Areas/{2}" + _options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in _options.SharedViewFolders)
                {
                    o.AreaViewLocationFormats.Add("/Areas/{2}" + _options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }

                o.AreaViewLocationFormats.Add("/Areas/Shared/Views/{0}" + RazorViewEngine.ViewExtension);
                o.AreaPageViewLocationFormats.Add("/Areas/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in _options.SharedViewFolders)
                {
                    o.AreaViewLocationFormats.Add("/Areas/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                    o.AreaPageViewLocationFormats.Add("/Areas/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }

                o.AreaViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);
                o.AreaPageViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/{0}" + RazorViewEngine.ViewExtension);

                foreach (var sharedViewFolder in _options.SharedViewFolders)
                {
                    o.AreaViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                    o.AreaPageViewLocationFormats.Add(_options.RootFeatureFolder + "/Shared/Views/" + sharedViewFolder + "/{0}" + RazorViewEngine.ViewExtension);
                }
            }
        }
        #endregion

        #region View Location Expander
        public static IMvcBuilder AddViewLocationExpander(this IMvcBuilder builder, string mvcImplementationFolder = "Controllers/")
        {
            builder.Services.Configure<RazorViewEngineOptions>(options =>
           {
               options.ViewLocationExpanders.Insert(0, new ViewLocationExpander(mvcImplementationFolder));
           });

            return builder;
        }
        #endregion

        #region One Transaction per Http Call
        public static void UseOneTransactionPerHttpCall(this IServiceCollection serviceCollection, System.Transactions.IsolationLevel level = System.Transactions.IsolationLevel.ReadCommitted)
        {
            serviceCollection.AddScoped<TransactionScope>((serviceProvider) =>
            {
                var transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = level });
                return transactionScope;
            });

            serviceCollection.AddScoped(typeof(UnitOfWorkFilter), typeof(UnitOfWorkFilter));

            serviceCollection
                .AddMvc(setup =>
                {
                    setup.Filters.AddService<UnitOfWorkFilter>(1);
                });
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

        public static IServiceCollection AddValidationService(this IServiceCollection services)
        {
            return services.AddTransient<IValidationService, ValidationService>();
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
        /// Users the must be authorized. The authorization filter will be executed on any request that enters the MVC middleware and maps to a valid action.
        /// </summary>
        public static IMvcBuilder UserMustBeAuthorized(this IMvcBuilder builder, bool enabled = true)
        {
            if (enabled)
            {
                var services = builder.Services;

                services.Configure<MvcOptions>(options =>
                {
                    //https://ondrejbalas.com/authorization-options-in-asp-net-core/
                    //The authorization filter will be executed on any request that enters the MVC middleware and maps to a valid action.
                    var globalPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    //other requirements such as location
                    .Build();

                    options.Filters.Add(new AuthorizeFilter(globalPolicy));
                });
            }

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
                .OfType<SystemTextJsonInputFormatter>().FirstOrDefault();

                if (jsonInputFormatter != null)
                {
                    foreach (var mediaType in variableResourceRepresentationOptions.JsonInputMediaTypes)
                    {
                        jsonInputFormatter.SupportedMediaTypes.Add(mediaType);
                    }
                }

                var jsonInputFormatterNewtonsoft = options.InputFormatters
               .OfType<NewtonsoftJsonInputFormatter>().FirstOrDefault();

                if (jsonInputFormatterNewtonsoft != null)
                {
                    foreach (var mediaType in variableResourceRepresentationOptions.JsonInputMediaTypes)
                    {
                        jsonInputFormatterNewtonsoft.SupportedMediaTypes.Add(mediaType);
                    }
                }

                var jsonOutputFormatter = options.OutputFormatters
                   .OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();

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

                var jsonOutputFormatterNewtonsoft = options.OutputFormatters
                   .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatterNewtonsoft != null)
                {
                    foreach (var mediaType in variableResourceRepresentationOptions.JsonOutputMediaTypes)
                    {
                        jsonOutputFormatterNewtonsoft.SupportedMediaTypes.Add(mediaType);
                    }

                    if (variableResourceRepresentationOptions.RemoveTextJsonTextOutputFormatter)
                    {
                        // remove text/json as it isn't the approved media type
                        // for working with JSON at API level
                        if (jsonOutputFormatterNewtonsoft.SupportedMediaTypes.Contains("text/json"))
                        {
                            jsonOutputFormatterNewtonsoft.SupportedMediaTypes.Remove("text/json");
                        }
                    }
                }

                //.NET Core 2.2 
                //var jsonInputFormatterNewtonsoft = options.InputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();

                //if (jsonInputFormatterNewtonsoft != null)
                //{
                //    foreach (var mediaType in variableResourceRepresentationOptions.JsonInputMediaTypes)
                //    {
                //        jsonInputFormatterNewtonsoft.SupportedMediaTypes.Add(mediaType);
                //    }
                //}

                //var jsonOutputFormatterNewtonsoft = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();

                //if (jsonOutputFormatterNewtonsoft != null)
                //{
                //    foreach (var mediaType in variableResourceRepresentationOptions.JsonOutputMediaTypes)
                //    {
                //        jsonOutputFormatterNewtonsoft.SupportedMediaTypes.Add(mediaType);
                //    }

                //    if (variableResourceRepresentationOptions.RemoveTextJsonTextOutputFormatter)
                //    {
                //        // remove text/json as it isn't the approved media type
                //        // for working with JSON at API level
                //        if (jsonOutputFormatterNewtonsoft.SupportedMediaTypes.Contains("text/json"))
                //        {
                //            jsonOutputFormatterNewtonsoft.SupportedMediaTypes.Remove("text/json");
                //        }
                //    }
                //}

                foreach (var formatterMapping in variableResourceRepresentationOptions.FormatterMappings)
                {
                    options.FormatterMappings.SetMediaTypeMappingForFormat(formatterMapping.Key, formatterMapping.Value);
                }
            });

            return builder;
        }
        #endregion

        #region Hosted Services
        public static IServiceCollection AddProcessingService<TWorkItem>(this IServiceCollection services, TimeSpan interval)
        where TWorkItem : class, IScopedProcessingService
        {
            return services.AddTransient<IHostedService>(sp =>
            {
                var logger = sp.GetService<ILogger<IntervalHostedService<TWorkItem>>>();
                return new IntervalHostedService<TWorkItem>(sp, logger, interval);
            });
        }

        public static IServiceCollection AddHostedServiceScoped<TWorkItem>(this IServiceCollection services, params object[] arguments)
            where TWorkItem : class, IScopedProcessingService
        {
            return services.AddTransient<IHostedService>(sp =>
            {
                return new ScopedHostedService<TWorkItem>(sp)
                {
                    Arguments = arguments
                };
            });
        }

        /// <summary>
        /// Adds the hosted service with cron job schedules or use [CronJob] to the application.
        /// </summary>
        public static IServiceCollection AddHostedServiceCronJob<TWorkItem>(this IServiceCollection services, params string[] cronSchedules)
            where TWorkItem : class, IScopedProcessingService
        {
            return services.AddTransient<IHostedService>(sp =>
            {
                var logger = sp.GetService<ILogger<CronHostedService<TWorkItem>>>();
                return new CronHostedService<TWorkItem>(sp, logger, cronSchedules);
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

        public static IServiceCollection AddHostedServiceBackgroundTaskQueue(this IServiceCollection services, Action<QueuedHostedServiceOptions> configure)
        {
            services.Configure(configure);
            return services.AddHostedServiceBackgroundTaskQueue();
        }

        public static IServiceCollection AddFileProcessing<T, TResultProcessor>(this IServiceCollection services) where TResultProcessor : class, IResultProcessor<T>
        {
            services.TryAddSingleton<ICsvResultParser<T>, CsvResultParser<T>>();
            services.TryAddScoped<IResultProcessor<T>, TResultProcessor>();
            services.AddSingleton<FileProcessingChannel<T>>();
            services.AddHostedService<FileProcessingService<T>>();

            return services;
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

            if (supportAllLanguagesFormatting || supportAllCountryFormatting)
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

            //Url Localization
            //https://www.strathweb.com/2015/11/localized-routes-with-asp-net-5-and-mvc-6/
            //https://github.com/saaratrix/asp.net-core-mvc-localized-routing
            //https://www.strathweb.com/2019/08/dynamic-controller-routing-in-asp-net-core-3-0/

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
                     new UrlRequestCultureProvider(),
                     new QueryStringRequestCultureProvider() { QueryStringKey = "culture", UIQueryStringKey = "ui-culture" },
                     new CookieRequestCultureProvider(),
                     new AcceptLanguageHeaderRequestCultureProvider(),
                };
            });

            services.AddSingleton(sp => sp.GetService<IOptions<RequestLocalizationOptions>>().Value);

            return services;
        }
        #endregion

        #region Azure Key Vault Config
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

                //https://joonasw.net/view/azure-ad-managed-service-identity
                //https://joonasw.net/view/aspnet-core-azure-keyvault-msi
                //https://anthonychu.ca/post/secrets-aspnet-core-key-vault-msi/
                //https://kasunkodagoda.com/2018/04/28/using-managed-service-identity-to-access-azure-key-vault-from-azure-app-service/
                //Enable Managed Service Identity on the Web App. Settings > Identity > System assigned > On
                //Allow the generated Service Principal access to the Production Key Vault. Key Vault > Access Policies > Secret permissions > Get and List

                //https://joonasw.net/view/aspnet-core-azure-keyvault-msi
                //If used outside Azure, it will authenticate as the developer's user. It will try using Azure CLI 2.0 (install from here). The second option is AD Integrated Authentication.
                //After installing the CLI, remember to run az login, and login to your Azure account before running the app.Another important thing is that you need to also select the subscription where the Key Vault is.So if you have access to more than one subscription, also run az account set - s "My Azure Subscription name or id"
                //Then you need to make sure your user has access to a Key Vault(does not have to be the production vault...).
                if (!string.IsNullOrWhiteSpace(vaultName) && (!useOnlyInProduction || ctx.HostingEnvironment.IsProduction()))
                {
                    //Section:Name = Section--Name

                    //Create Managed Service Identity (MSI) token provider
                    //var tokenProvider = new AzureServiceTokenProvider();
                    //var kvClient = new KeyVaultClient((authority, resource, scope) => tokenProvider.KeyVaultTokenCallback(authority, resource, scope));
                    var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));
                    builder.AddAzureKeyVault($"https://{vaultName}.vault.azure.net", kvClient, new DefaultKeyVaultSecretManager());
                }
            });
        }

        public static IHostBuilder UseAzureKeyVault(this IHostBuilder hostBuilder, string vaultName = null, bool useOnlyInProduction = true)
        {
            return hostBuilder.ConfigureAppConfiguration((ctx, builder) =>
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

                //https://joonasw.net/view/azure-ad-managed-service-identity
                //https://joonasw.net/view/aspnet-core-azure-keyvault-msi
                //https://anthonychu.ca/post/secrets-aspnet-core-key-vault-msi/
                //https://kasunkodagoda.com/2018/04/28/using-managed-service-identity-to-access-azure-key-vault-from-azure-app-service/
                //Enable Managed Service Identity on the Web App. Settings > Identity > System assigned > On
                //Allow the generated Service Principal access to the Production Key Vault. Key Vault > Access Policies > Secret permissions > Get

                //If used outside Azure, it will authenticate as the developer's user. It will try using Azure CLI 2.0 (install from here). The second option is AD Integrated Authentication.
                //After installing the CLI, remember to run az login, and login to your Azure account before running the app.Another important thing is that you need to also select the subscription where the Key Vault is.So if you have access to more than one subscription, also run az account set - s "My Azure Subscription name or id"
                //Then you need to make sure your user has access to a Key Vault(does not have to be the production vault...).
                if (!string.IsNullOrWhiteSpace(vaultName) && (!useOnlyInProduction || ctx.HostingEnvironment.IsProduction()))
                {
                    //Section:Name = Section--Name

                    //Create Managed Service Identity (MSI) token provider
                    //var tokenProvider = new AzureServiceTokenProvider();
                    //var kvClient = new KeyVaultClient((authority, resource, scope) => tokenProvider.KeyVaultTokenCallback(authority, resource, scope));
                    var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));
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

        #region EF Config
        public static IWebHostBuilder UseEFConfiguration<TDbContext>(this IWebHostBuilder webHostBuilder, string connectionStringOrName, bool initializeSchema = false, bool useOnlyInProduction = true)
        where TDbContext : DbContext, IConfigurationDbContext
        {
            return webHostBuilder.ConfigureAppConfiguration((ctx, builder) =>
            {
                if ((!useOnlyInProduction || ctx.HostingEnvironment.IsProduction()))
                {
                    var config = builder.Build();

                    if (config.GetSection("ConnectionStrings").GetChildren().Any(c => c.Key == connectionStringOrName))
                    {
                        builder.AddEFConfiguration<TDbContext>(config.GetConnectionString(connectionStringOrName), initializeSchema);
                    }
                    else
                    {
                        builder.AddEFConfiguration<TDbContext>(connectionStringOrName, initializeSchema);
                    }
                }
            });
        }

        public static IHostBuilder UseEFConfiguration<TDbContext>(this IHostBuilder webHostBuilder, string connectionStringOrName, bool initializeSchema = false, bool useOnlyInProduction = true)
       where TDbContext : DbContext, IConfigurationDbContext
        {
            return webHostBuilder.ConfigureAppConfiguration((ctx, builder) =>
            {
                if ((!useOnlyInProduction || ctx.HostingEnvironment.IsProduction()))
                {
                    var config = builder.Build();

                    if (config.GetSection("ConnectionStrings").GetChildren().Any(c => c.Key == connectionStringOrName))
                    {
                        builder.AddEFConfiguration<TDbContext>(config.GetConnectionString(connectionStringOrName), initializeSchema);
                    }
                    else
                    {
                        builder.AddEFConfiguration<TDbContext>(connectionStringOrName, initializeSchema);
                    }
                }
            });
        }
        #endregion

        #region Json string Config
        public static IConfigurationBuilder AddJsonString(this IConfigurationBuilder builder, string json)
        {

            return builder.AddJsonStream(StringToStream(json));
        }

        private static Stream StringToStream(string str)
        {
            var memStream = new MemoryStream();
            var textWriter = new StreamWriter(memStream);
            textWriter.Write(str);
            textWriter.Flush();
            memStream.Seek(0, SeekOrigin.Begin);

            return memStream;
        }
        #endregion

        #region Startup Tasks

        public static IWebHostBuilder UseStartupTasks(this IWebHostBuilder builder, bool scanApplicationDependencies = true)
        {
            return builder.ConfigureServices((services) =>
            {
                services.AddTaskExecutingServer();
                if (scanApplicationDependencies)
                {
                    services.AddDbStartupTasks();
                    services.AddStartupTasks();
                }
            });
        }

        public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services)
        where TStartupTask : class, IStartupTask
        {
            _sharedContext.RegisterTask();

            return services
           .AddTransient<IStartupTask, TStartupTask>()
           .AddTaskExecutingServer();
        }

        public static IServiceCollection AddDbStartupTask<TDbStartupTask>(this IServiceCollection services)
        where TDbStartupTask : class, IDbStartupTask
        {
            _sharedContext.RegisterTask();

            return services
          .AddTransient<IDbStartupTask, TDbStartupTask>()
          .AddTaskExecutingServer();
        }

        public static IServiceCollection AddDbInitializeStartupTask<TDbContext>(this IServiceCollection services, Action<TDbContext> configure, int order = 0)
        where TDbContext : class
        {
            _sharedContext.RegisterTask();

            return services
          .AddTransient<IDbStartupTask>(sp => new DbInitializeStartupTask<TDbContext>(sp, configure, order))
          .AddTaskExecutingServer();
        }

        public static IServiceCollection AddDbInitializeHostedService<TDbContext>(this IServiceCollection services, Action<TDbContext> configure)
        where TDbContext : class
        {
            return services.AddHostedService<IHostedService>(sp => new DbInitializeHostedService<TDbContext>(sp, configure));
        }

        public static IServiceCollection AddDbStartupTasks(this IServiceCollection services, Action<StartupTaskOptions> setup = null)
        {
            var options = new StartupTaskOptions();
            if (setup != null)
                setup(options);

            if (options.LoadApplicationDependencies)
            {
                services.LoadApplicationDependencies();
            }

            if (!string.IsNullOrEmpty(options.LoadPathDependencies))
            {
                services.LoadAssembliesFromPath(options.LoadPathDependencies);
            }

            services.Scan(scan =>
            {
                scan.FromCurrentDomainAssemblies(options.Predicate)
                .AddClasses(c => c.AssignableTo<IDbStartupTask>().Where(_ => _sharedContext.RegisterTask()))
                .As<IDbStartupTask>()
                .WithTransientLifetime();
            });

            services.AddTaskExecutingServer();

            return services;
        }

        public static IServiceCollection AddStartupTasks(this IServiceCollection services, Action<StartupTaskOptions> setup = null)
        {
            var options = new StartupTaskOptions();
            if (setup != null)
                setup(options);

            if (options.LoadApplicationDependencies)
            {
                services.LoadApplicationDependencies();
            }

            if (!string.IsNullOrEmpty(options.LoadPathDependencies))
            {
                services.LoadAssembliesFromPath(options.LoadPathDependencies);
            }

            services.Scan(scan =>
            {
                scan.FromCurrentDomainAssemblies(options.Predicate)
                .AddClasses(c => c.AssignableTo<IStartupTask>().Where(_ => _sharedContext.RegisterTask()))
                .As<IStartupTask>()
                .WithTransientLifetime();
            });


            services.AddTaskExecutingServer();
            return services;
        }


        private static readonly StartupTaskContext _sharedContext = new StartupTaskContext();
        //Only needed for .NET Core 2.2 as in .NET Core 3.0 IHostedServices run before Server Starts. Hosted Services are run in the order they are added so Db StartupTask HostedServices must be added before StartupTask HostedServices and standard HostedServices.
        public static IServiceCollection AddTaskExecutingServer(this IServiceCollection services)
        {
            if (services.Any(service => service.ImplementationType == typeof(StartupTasksHostedService)))
            {
                return services;
            }

            services.AddSingleton(_sharedContext);

            services.AddTransient<StartupTasksHostedService>();

            return services.AddHostedService<StartupTasksHostedService>();

            //.NET Core 2.2 
            //var decoratorType = typeof(TaskExecutingServer);
            //if (services.Any(service => service.ImplementationType == decoratorType))
            //{
            //    // We've already decorated the IServer
            //    return services;
            //}

            //services.AddTransient<StartupTasksHostedService>();

            //services.AddSingleton(_sharedContext);

            ////Replace IISServerSetupFilter
            //var iisServerSetupFilter = services.FirstOrDefault(s => s.ImplementationInstance != null && s.ImplementationInstance.GetType().Name == "IISServerSetupFilter");
            //if (iisServerSetupFilter != null)
            //{
            //    var virtualPath = (string)iisServerSetupFilter.ImplementationInstance.GetType().GetField("_virtualPath", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(iisServerSetupFilter.ImplementationInstance);
            //    var index = services.IndexOf(iisServerSetupFilter);
            //    services[index] = ServiceDescriptor.Singleton(typeof(IStartupFilter), new TaskExecutingServerIISServerSetupFilter(virtualPath));
            //}

            //// Decorate the IServer with our TaskExecutingServer
            //return services.Decorate<IServer, TaskExecutingServer>();
        }
        #endregion

        #region WebHostBuilder Extensions
        public static IWebHostBuilder ConfigureWebHost(this IWebHostBuilder webHostBuilder, Action<IWebHostBuilder> configure)
        {
            configure(webHostBuilder);

            return webHostBuilder;
        }
        #endregion

        #region Elastic Search Extensions
        public static void AddElasticSearch(this IServiceCollection services, string url, string defaultIndex = "default")
        {
            var settings = new ConnectionSettings(new Uri(url))
                .PrettyJson()
                .BasicAuthentication("elastic", "elastic")
                .DefaultIndex(defaultIndex);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }
        #endregion

        #region DbContext Extensions
        //        public static IServiceCollection AddDbContextNoSql<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContextNoSql
        //        {
        //            if (ConnectionStringHelper.IsLiteDbInMemory(connectionString))
        //            {
        //                contextLifetime = ServiceLifetime.Singleton;
        //            }

        //            if (ConnectionStringHelper.IsLiteDbInMemory(connectionString))
        //            {
        //                services.AddDbContextNoSqlInMemory<TContext>(contextLifetime);
        //            }
        //            else
        //            {
        //                services.Add(new ServiceDescriptor(typeof(TContext), sp => ActivatorUtilities.CreateInstance(sp, typeof(TContext), new object[] { connectionString }), contextLifetime));
        //            }
        //            return services;
        //        }

        //        public static IServiceCollection AddDbContextNoSqlInMemory<TContext>(this IServiceCollection services, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContextNoSql
        //        {
        //            services.Add(new ServiceDescriptor(typeof(TContext), sp => ActivatorUtilities.CreateInstance(sp, typeof(TContext), new object[] { new MemoryStream() }), contextLifetime));
        //            return services;
        //        }

        //        public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        //        {
        //            if (ConnectionStringHelper.IsSQLiteInMemory(connectionString))
        //            {
        //                contextLifetime = ServiceLifetime.Singleton;
        //            }

        //            return services.AddDbContext<TContext>(options =>
        //            {
        //                options.SetConnectionString<TContext>(connectionString);
        //                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        //            }, contextLifetime);
        //        }

        //        public static DbContextOptionsBuilder SetConnectionString<TContext>(this DbContextOptionsBuilder options, string connectionString, string migrationsAssembly = "")
        //            where TContext : DbContext
        //        {
        //            if (connectionString == null)
        //            {
        //                return options;
        //            }
        //            else if (connectionString == string.Empty)
        //            {
        //                return options.UseInMemoryDatabase(typeof(TContext).FullName);
        //            }
        //            else if (ConnectionStringHelper.IsSQLite(connectionString))
        //            {
        //                if (!string.IsNullOrWhiteSpace(migrationsAssembly))
        //                {
        //                    return options.UseSqlite(connectionString, sqlOptions =>
        //                    {
        //                        sqlOptions.MigrationsAssembly(migrationsAssembly);
        //                        sqlOptions.UseNetTopologySuite();
        //                    });
        //                }
        //                return options.UseSqlite(connectionString, sqlOptions =>
        //                {
        //                    sqlOptions.UseNetTopologySuite();
        //                });
        //            }
        //#if NETCOREAPP3_1
        //            else if (ConnectionStringHelper.IsCosmos(connectionString))
        //            {
        //                var dbConnectionString = new CosmosDBConnectionString(connectionString);
        //                return options.UseCosmos(dbConnectionString.ServiceEndpoint.ToString(), dbConnectionString.AuthKey, null);
        //            }
        //#endif
        //            else
        //            {
        //                if (!string.IsNullOrWhiteSpace(migrationsAssembly))
        //                {
        //                    return options.UseSqlServer(connectionString, sqlOptions =>
        //                    {
        //                        sqlOptions.MigrationsAssembly(migrationsAssembly);
        //                        sqlOptions.UseNetTopologySuite();
        //                    });
        //                }
        //                return options.UseSqlServer(connectionString, sqlOptions =>
        //                {
        //                    sqlOptions.UseNetTopologySuite();
        //                });
        //            }
        //        }

        //        //https://medium.com/volosoft/asp-net-core-dependency-injection-best-practices-tips-tricks-c6e9c67f9d96
        //        public static IServiceCollection AddDbContextInMemory<TContext>(this IServiceCollection services, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        //        {
        //            return services.AddDbContext<TContext>(options =>
        //                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()), contextLifetime);
        //        }

        //        public static IServiceCollection AddDbContextSqlServer<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        //        {
        //            return services.AddDbContext<TContext>(options =>
        //                    options.UseSqlServer(connectionString, sqlOptions =>
        //                    {
        //                        sqlOptions.UseNetTopologySuite();
        //                    }), contextLifetime);
        //        }

        //        public static IServiceCollection AddDbContextSqlite<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        //        {
        //            return services.AddDbContext<TContext>(options =>
        //                    options.UseSqlite(connectionString, sqlOptions =>
        //                    {
        //                        sqlOptions.UseNetTopologySuite();
        //                    }), contextLifetime);
        //        }

        //        public static IServiceCollection AddDbContextSqliteInMemory<TContext>(this IServiceCollection services, ServiceLifetime contextLifetime = ServiceLifetime.Singleton) where TContext : DbContext
        //        {
        //            return services.AddDbContext<TContext>(options =>
        //                    options.UseSqlite(":memory:", sqlOptions =>
        //                    {
        //                        sqlOptions.UseNetTopologySuite();
        //                    }), contextLifetime);
        //        }

        //        public static IServiceCollection AddDbContextPoolSqlServer<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        //        {
        //            return services.AddDbContextPool<TContext>(options =>
        //                    options.UseSqlServer(connectionString, sqlOptions =>
        //                    {
        //                        sqlOptions.UseNetTopologySuite();
        //                    }));
        //        }

        //        public static IServiceCollection AddDbContextSqlServerWithRetries<TContext>(this IServiceCollection services, string connectionString, int retries = 10, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        //        {
        //            return services.AddDbContext<TContext>(options =>
        //                     options.UseSqlServer(connectionString,
        //                     sqlServerOptionsAction: sqlOptions =>
        //                     {
        //                         sqlOptions.EnableRetryOnFailure(
        //                         maxRetryCount: retries,
        //                         maxRetryDelay: TimeSpan.FromSeconds(30),
        //                         errorNumbersToAdd: null);
        //                         sqlOptions.UseNetTopologySuite();
        //                     }), contextLifetime);
        //        }

        //Repository needs interface so it can be injected into Domain Services
        public static void AddRepository<TRepository, TRepositoryImplementation>(this IServiceCollection services)
        where TRepository : class
        where TRepositoryImplementation : class, TRepository
        {
            services.AddScoped<TRepositoryImplementation>();
            services.AddScoped<TRepository>(sp => sp.GetService<TRepositoryImplementation>());
        }

        public static void AddUnitOfWork<TUnitOfWorkImplementation>(this IServiceCollection services)
        where TUnitOfWorkImplementation : class, IUnitOfWork
        {
            services.AddScoped<TUnitOfWorkImplementation>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetService<TUnitOfWorkImplementation>());
        }

        public static void AddUnitOfWork<TUnitOfWork, TUnitOfWorkImplementation>(this IServiceCollection services)
            where TUnitOfWork : class, IUnitOfWork
            where TUnitOfWorkImplementation: class, TUnitOfWork
        {
            services.AddScoped<TUnitOfWorkImplementation>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetService<TUnitOfWorkImplementation>());
            services.AddScoped<TUnitOfWork>(sp => sp.GetService<TUnitOfWorkImplementation>());
        }
        #endregion

        #region MiniProfiler
        public static IServiceCollection AddMiniProfiler(this IServiceCollection services, string connectionString, bool initializeDatabase)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return services.AddMiniProfilerInMemory();
            }
            else if (ConnectionStringHelper.IsSQLite(connectionString))
            {
                return services.AddMiniProfilerSqlLite(connectionString, initializeDatabase);
            }
            else
            {
                return services.AddMiniProfilerSqlServer(connectionString, initializeDatabase);
            }
        }

        public static IServiceCollection AddMiniProfilerInMemory(this IServiceCollection services)
        {
            services.AddMiniProfiler(options =>
            {
                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;

                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.PopupStartHidden = true; //ALT + P to display
                options.PopupShowTrivial = true;
                options.PopupShowTimeWithChildren = true;
                options.ResultsAuthorize = (request) => true;
                options.UserIdProvider = (request) => request.HttpContext.User.Identity.Name;
            }).AddEntityFramework();

            return services;
        }

        public static IServiceCollection AddMiniProfilerSqlServer(this IServiceCollection services, string connectionString, bool initializeDatabase)
        {
            services.AddMiniProfiler(options =>
            {
                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;

                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.PopupStartHidden = true; //ALT + P to display
                options.PopupShowTrivial = true;
                options.PopupShowTimeWithChildren = true;
                options.ResultsAuthorize = (request) => true;
                options.UserIdProvider = (request) => request.HttpContext.User.Identity.Name;

                options.Storage = new SqlServerStorage(connectionString);
            }).AddEntityFramework();

            if (initializeDatabase)
            {
                MiniProfilerInitializer.EnsureDbAndTablesCreatedAsync(connectionString).Wait();
            }

            return services;
        }

        public static IServiceCollection AddMiniProfilerSqlLite(this IServiceCollection services, string connectionString, bool initializeDatabase)
        {
            services.AddMiniProfiler(options =>
            {
                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;

                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.PopupStartHidden = true; //ALT + P to display
                options.PopupShowTrivial = true;
                options.PopupShowTimeWithChildren = true;
                options.ResultsAuthorize = (request) => true;
                options.UserIdProvider = (request) => request.HttpContext.User.Identity.Name;

                options.Storage = new SqliteStorage(connectionString);
            }).AddEntityFramework();

            if (initializeDatabase)
            {
                MiniProfilerInitializer.EnsureDbAndTablesCreatedAsync(connectionString).Wait();
            }

            return services;
        }
        #endregion

        #region Authentication

        public static AuthenticationBuilder AddJwtAuthentication(this AuthenticationBuilder authenticationBuilder,
           string bearerTokenKey,
           string bearerTokenPublicSigningKeyPath,
           string bearerTokenPublicSigningCertificatePath,
           string bearerTokenExternalIssuers,
           string bearerTokenLocalIssuer,
           string bearerTokenAudiences)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // keep original claim types

            var signingKeys = new List<SecurityKey>();
            if (!String.IsNullOrWhiteSpace(bearerTokenKey))
            {
                //Symmetric
                signingKeys.Add(SigningKey.LoadSymmetricSecurityKey(bearerTokenKey));
            }

            if (!String.IsNullOrWhiteSpace(bearerTokenPublicSigningKeyPath))
            {
                //Assymetric
                signingKeys.Add(SigningKey.LoadPublicRsaSigningKey(bearerTokenPublicSigningKeyPath));
            }

            if (!String.IsNullOrWhiteSpace(bearerTokenPublicSigningCertificatePath))
            {
                //Assymetric
                signingKeys.Add(SigningKey.LoadPublicSigningCertificate(bearerTokenPublicSigningCertificatePath));
            }

            var validIssuers = new List<string>();
            if (!string.IsNullOrEmpty(bearerTokenExternalIssuers))
            {
                foreach (var externalIssuer in bearerTokenExternalIssuers.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(externalIssuer))
                    {
                        validIssuers.Add(externalIssuer);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(bearerTokenLocalIssuer))
            {
                validIssuers.Add(bearerTokenLocalIssuer);
            }

            var validAudiences = new List<string>();
            foreach (var audience in bearerTokenAudiences.Split(','))
            {
                if (!string.IsNullOrWhiteSpace(audience))
                {
                    validAudiences.Add(audience);
                }
            }

            //https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide
            //https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/46eb0ca2942831f858597b7fa73bb3230b6c16db/src/System.IdentityModel.Tokens.Jwt/JwtSecurityTokenHandler.cs
            //https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/e3389b0e2e5a428e6a9bfd72a377a1f415588a8b/src/Microsoft.IdentityModel.Tokens/Validators.cs
            //https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide
            //https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation does reference + JWT auth

            return authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                //cfg.Authority = "{yourAuthorizationServerAddress}";
                //cfg.Audience = "{yourAudience}";

                options.SaveToken = true; //var accessToken = await HttpContext.GetTokenAsync("access_token"); //forward the JWT in an outgoing request.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    // Specify what in the JWT needs to be checked 
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateTokenReplay = true,
                    ValidateActor = true,
                    ValidateIssuerSigningKey = true,

                    RequireSignedTokens = true,

                    ValidIssuers = validIssuers, //in the JWT this is the uri of the Identity Provider which issues the token.
                    ValidAudiences = validAudiences, //in the JWT this is aud. This is the resource the user is expected to have.

                    IssuerSigningKeys = signingKeys //Tries to match on 1. kid, 2. x5t (cert thumbprint), 3. attempt all
                };

                //https://docs.microsoft.com/en-us/dotnet/api/
                //microsoft.aspnetcore.authentication.jwtbearer.jwtbearerevents
                options.Events = new JwtBearerEvents
                {
                    OnForbidden = e =>
                    {
                        Log.Warning("API access was forbidden!");
                        return Task.FromResult(e);
                    },
                    OnAuthenticationFailed = e =>
                    {
                        Log.Warning(e.Exception, "Authentication Failed!");
                        return Task.FromResult(e);
                    }
                };
            });
        }

        public static void AddCookiePolicy(this IServiceCollection services, string cookieConsentName)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.ConsentCookie.Name = cookieConsentName;
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        public static void AddIdentity<TContext, TUser, TRole>(this IServiceCollection services,
        int maxFailedAccessAttemptsBeforeLockout,
        int lockoutMinutes,
        bool requireDigit,
        int requiredLength,
        int requiredUniqueChars,
        bool requireLowercase,
        bool requireNonAlphanumeric,
        bool requireUppercase,

        //user
        bool requireConfirmedEmail,
        bool requireUniqueEmail,
        int registrationEmailConfirmationExprireDays,
        int forgotPasswordEmailConfirmationExpireHours,
        int userDetailsChangeLogoutMinutes)
            where TContext : DbContext
            where TUser : class
            where TRole : class
        {
            services.AddIdentity<TUser, TRole>(options =>
            {
                options.Password.RequireDigit = requireDigit;
                options.Password.RequiredLength = requiredLength;
                options.Password.RequiredUniqueChars = requiredUniqueChars;
                options.Password.RequireLowercase = requireLowercase;
                options.Password.RequireNonAlphanumeric = requireNonAlphanumeric;
                options.Password.RequireUppercase = requireUppercase;
                options.User.RequireUniqueEmail = requireUniqueEmail;
                options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                options.Tokens.EmailConfirmationTokenProvider = "emailconf";

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = maxFailedAccessAttemptsBeforeLockout;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutMinutes);
            })
                .AddEntityFrameworkStores<TContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider<TUser>>("emailconf")
                .AddPasswordValidator<DoesNotContainPasswordValidator<TUser>>();

            //https://github.com/brunohbrito/JPProject.AspNetCore.PasswordHasher
            services.UpgradePasswordSecurity()
            .WithStrenghten(PasswordHasherStrenght.Sensitive)
            .UseArgon2<TUser>();

            //registration email confirmation days
            services.Configure<EmailConfirmationTokenProviderOptions>(options =>
           options.TokenLifespan = TimeSpan.FromDays(registrationEmailConfirmationExprireDays));

            //forgot password hours
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(forgotPasswordEmailConfirmationExpireHours));

            //Security stamp validator validates every x minutes and will log out user if account is changed. e.g password change
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(userDetailsChangeLogoutMinutes);
            });
        }

        public static void AddIdentityCore<TContext, TUser, TRole>(this IServiceCollection services,
        int maxFailedAccessAttemptsBeforeLockout,
        int lockoutMinutes,
        bool requireDigit,
        int requiredLength,
        int requiredUniqueChars,
        bool requireLowercase,
        bool requireNonAlphanumeric,
        bool requireUppercase,

        //user
        bool requireConfirmedEmail,
        bool requireUniqueEmail,
        int registrationEmailConfirmationExprireDays,
        int forgotPasswordEmailConfirmationExpireHours,
        int userDetailsChangeLogoutMinutes)
            where TContext : DbContext
            where TUser : class
            where TRole : class
        {
            services.AddIdentityCore<TUser>(options =>
            {
                options.Password.RequireDigit = requireDigit;
                options.Password.RequiredLength = requiredLength;
                options.Password.RequiredUniqueChars = requiredUniqueChars;
                options.Password.RequireLowercase = requireLowercase;
                options.Password.RequireNonAlphanumeric = requireNonAlphanumeric;
                options.Password.RequireUppercase = requireUppercase;
                options.User.RequireUniqueEmail = requireUniqueEmail;
                options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                options.Tokens.EmailConfirmationTokenProvider = "emailconf";

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = maxFailedAccessAttemptsBeforeLockout;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutMinutes);
            })
             .AddRoles<TRole>()
                 .AddEntityFrameworkStores<TContext>()
                 .AddDefaultTokenProviders()
                 .AddTokenProvider<EmailConfirmationTokenProvider<TUser>>("emailconf")
                 .AddPasswordValidator<DoesNotContainPasswordValidator<TUser>>()
                 .AddRoleValidator<RoleValidator<IdentityRole>>()
                 .AddRoleManager<RoleManager<IdentityRole>>()
                 .AddSignInManager<SignInManager<TUser>>();

            //https://github.com/brunohbrito/JPProject.AspNetCore.PasswordHasher
            services.UpgradePasswordSecurity()
            .WithStrenghten(PasswordHasherStrenght.Sensitive)
            .UseArgon2<TUser>();

            //registration email confirmation days
            services.Configure<EmailConfirmationTokenProviderOptions>(options =>
           options.TokenLifespan = TimeSpan.FromDays(registrationEmailConfirmationExprireDays));

            //forgot password hours
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(forgotPasswordEmailConfirmationExpireHours));

            //Security stamp validator validates every x minutes and will log out user if account is changed. e.g password change
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(userDetailsChangeLogoutMinutes);
            });
        }

        #endregion

        #region CORS
        public static void ConfigureCorsAllowAnyOrigin(this IServiceCollection services, string name)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name,
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        public static void ConfigureCorsAllowSpecificOrigin(this IServiceCollection services, string name, params string[] domains)
        {
            services.AddCors(options =>
            {
                //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.1
                options.AddPolicy(name,
              builder => builder
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .WithOrigins(domains)
              .AllowAnyMethod()
              .AllowAnyHeader());
            });
        }
        #endregion

        #region SignalR Hub Mapper
        public static IServiceCollection AddSignalRHubMapper(this IServiceCollection services, Action<SignalRHubMapperOptions> setup = null)
        {
            services.TryAddSingleton<ISignalRHubMapper, SignalRHubMapper>();

            var options = new SignalRHubMapperOptions();
            if (setup != null)
            {
                setup(options);
                services.Configure(setup);
            }

            if (!string.IsNullOrEmpty(options.LoadPathDependencies))
            {
                services.LoadAssembliesFromPath(options.LoadPathDependencies);
            }

            if (options.LoadApplicationDependencies)
            {
                services.LoadApplicationDependencies();
            }

            services.Scan(scan =>
            {
                scan.FromCurrentDomainAssemblies(options.Predicate)
                .AddClasses(c => c.AssignableTo<ISignalRHubMap>())
                .As<ISignalRHubMap>()
                .WithSingletonLifetime();
            });

            return services;
        }
        #endregion

        #region Dependency Injection by Convention
        //https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
        public static IServiceCollection AddServicesByConvention(this IServiceCollection services, Action<ServicesByConventionOptions> setup = null)
        {
            var options = new ServicesByConventionOptions();
            if (setup != null)
                setup(options);

            if (options.LoadApplicationDependencies)
            {
                services.LoadApplicationDependencies();
            }

            if (!string.IsNullOrEmpty(options.LoadPathDependencies))
            {
                services.LoadAssembliesFromPath(options.LoadPathDependencies);
            }

            services.Scan(scan =>
            {
                scan.FromCurrentDomainAssemblies(options.Predicate)
               .AddClasses(c => c.Where(t => t.Name.Contains("Singleton")))
               .UsingRegistrationStrategy(RegistrationStrategy.Skip)
               .AsMatchingInterface()
               .WithSingletonLifetime()
               .AddClasses(c => c.Where(t => t.Name.Contains("Scoped")))
               .UsingRegistrationStrategy(RegistrationStrategy.Skip)
               .AsMatchingInterface()
               .WithScopedLifetime()
               .AddClasses()
               .UsingRegistrationStrategy(RegistrationStrategy.Skip)
               .AsMatchingInterface()
               .WithTransientLifetime();
            });

            return services;
        }
        #endregion

        #region AutoMapper Interfaces
        /// <summary>
        /// Adds the automapper interfaces to the application.
        /// </summary>
        public static IServiceCollection AddAutoMapperInterfaces(this IServiceCollection services, Action<MappingOptions> setup = null)
        {
            var options = new MappingOptions();
            if (setup != null)
                setup(options);

            if (options.LoadApplicationDependencies)
            {
                services.LoadApplicationDependencies();
            }

            if (!string.IsNullOrEmpty(options.LoadPathDependencies))
            {
                services.LoadAssembliesFromPath(options.LoadPathDependencies);
            }

            services.AddSingleton(sp => new MapperConfiguration(cfg =>
            {
                //https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping
                //AutoMapper.Extensions.ExpressionMapping
                cfg.AddExpressionMapping();

                //AutoMapper.Collection & Automapper.Collection.EntityFrameworkCore
                //Allows a DTO model for owned Collection Types.
                cfg.AddCollectionMappers();
                //cfg.SetGeneratePropertyMaps<GenerateEntityFrameworkPrimaryKeyPropertyMaps<DB>>();
                new AutoMapperConfiguration(cfg, options.Predicate);
            }));
            services.AddSingleton<AutoMapper.IConfigurationProvider>(sp => sp.GetRequiredService<MapperConfiguration>());
            services.AddSingleton<IExpressionBuilder>(sp => new ExpressionBuilder(sp.GetRequiredService<MapperConfiguration>()));
            services.AddSingleton<IMapper>(sp => sp.GetRequiredService<MapperConfiguration>().CreateMapper());

            return services;
        }
        #endregion

        #region Health Checks

        public static IServiceCollection AddHealthCheckPublisher(this IServiceCollection services, Action<HealthCheckGenericPublisherOptions> configure)
        {
            services.Configure(configure);
            return services.AddSingleton<IHealthCheckPublisher, HealthCheckGenericPublisher>();
        }

        public static IHealthChecksBuilder AddSystemMemoryCheck(this IHealthChecksBuilder builder, string name = "Memory")
        {
            return builder.AddCheck<SystemMemoryHealthCheck>(name);
        }

        public static IHealthChecksBuilder AddPingCheck(this IHealthChecksBuilder builder, string name, string host, int timeout, int pingInterval = 0)
        {
            return builder.AddCheck(name, new PingHealthCheck(host, timeout, pingInterval)); ;
        }

        public static IHealthChecksBuilder AddFilePathWriteHealthCheck(this IHealthChecksBuilder builder, string filePath,
            HealthStatus failureStatus, IEnumerable<string> tags = default)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return builder.Add(new HealthCheckRegistration(
                "File Path Health Check",
                new FilePathWriteHealthCheck(filePath),
                failureStatus,
                tags));
        }
        #endregion

        #region Scrutor Scan
        public static IImplementationTypeSelector FromCurrentDomainAssemblies(this ITypeSourceSelector scan)
        {
            return scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        }

        public static IImplementationTypeSelector FromCurrentDomainAssemblies(this ITypeSourceSelector scan, Func<Assembly, bool> predicate)
        {
            return scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies().Where(predicate));
        }
        #endregion

        #region Scrutor Load into Current AppDomain
        public static IServiceCollection LoadApplicationDependencies(this IServiceCollection services, bool includeFramework = false)
        {
            return services.Scan(scan => scan.LoadApplicationDependencies(includeFramework));
        }

        public static ITypeSourceSelector LoadApplicationDependencies(this ITypeSourceSelector scan, bool includeFramework = false)
        {
            return scan.LoadApplicationDependencies(DependencyContext.Default, includeFramework);
        }

        public static IServiceCollection LoadApplicationDependencies(this IServiceCollection services, DependencyContext context, bool includeFramework = false)
        {
            return services.Scan(scan => scan.LoadApplicationDependencies(context, includeFramework));
        }

        public static ITypeSourceSelector LoadApplicationDependencies(this ITypeSourceSelector scan, DependencyContext context, bool includeFramework = false)
        {
            // Storage to ensure not loading the same assembly twice.
            Dictionary<string, bool> loaded = new Dictionary<string, bool>();

            // Filter to avoid loading all the .net framework
            bool ShouldLoad(string assemblyName)
            {
                return (includeFramework || NotNetFramework(assemblyName))
                    && !loaded.ContainsKey(assemblyName);
            }

            bool NotNetFramework(string assemblyName)
            {
                return !assemblyName.StartsWith("Microsoft.")
                    && !assemblyName.StartsWith("System")
                    && !assemblyName.StartsWith("Newtonsoft.")
                    && !assemblyName.StartsWith("netstandard")
                    && !assemblyName.StartsWith("Remotion.Linq")
                    && !assemblyName.StartsWith("SOS.NETCore")
                    && !assemblyName.StartsWith("WindowsBase")
                    && !assemblyName.StartsWith("mscorlib");
            }

            // Populate already loaded assemblies
            System.Diagnostics.Debug.WriteLine($">> Already loaded assemblies:");
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => ShouldLoad(a.GetName().Name)))
            {
                loaded.Add(a.GetName().Name, true);
                System.Diagnostics.Debug.WriteLine($">>>> {a.FullName}");
            }
            int alreadyLoaded = loaded.Keys.Count();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            var assemblies = context.RuntimeLibraries
              .SelectMany(library => library.GetDefaultAssemblyNames(context))
              .Where(an => ShouldLoad(an.FullName))
              .Select(an =>
              {
                  loaded.Add(an.FullName, true);
                  System.Diagnostics.Debug.WriteLine($"\n>> Referenced assembly => {an.FullName}");
                  return Assembly.Load(an);
              })
              .ToList();

            // Debug
            System.Diagnostics.Debug.WriteLine($"\n>> Assemblies loaded after scan ({(loaded.Keys.Count - alreadyLoaded)} assemblies in {sw.ElapsedMilliseconds} ms):");
            foreach (var a in loaded.Keys.OrderBy(k => k))
                System.Diagnostics.Debug.WriteLine($">>>> {a}");

            return scan;
        }

        public static IServiceCollection LoadAssembliesFromPath(this IServiceCollection services, string path)
        {
            return services.Scan(scan => scan.LoadAssembliesFromPath(path));
        }

        public static ITypeSourceSelector LoadAssembliesFromPath(this ITypeSourceSelector scan, string path)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assemblyPath in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => new[] { ".dll" }.Any(file.ToLower().EndsWith)))
            {
                System.Diagnostics.Debug.WriteLine($"Loading Assembly: {assemblyPath}");
                var assembly = Assembly.LoadFrom(assemblyPath);
                assemblies.Add(assembly);
            }
            return scan;
        }

        //https://github.com/dotnet/samples/blob/master/core/extensions/AppWithPlugin/AppWithPlugin/Program.cs
        //Without using AssemblyDependencyResolver, it is extremely difficult to correctly load plugins that have their own dependencies.   
        //By using AssemblyDependencyResolver along with a custom AssemblyLoadContext, an application can load plugins so that each plugin's dependencies are loaded from the correct location, and one plugin's dependencies will not conflict with another.This sample includes plugins that have conflicting dependencies and plugins that rely on satellite assemblies or native libraries.
        public static IServiceCollection LoadPluginAssembliesFromPath(this IServiceCollection services, string path)
        {
            return services.Scan(scan => scan.LoadPluginAssembliesFromPath(path));
        }

        public static ITypeSourceSelector LoadPluginAssembliesFromPath(this ITypeSourceSelector scan, string path)
        {
            foreach (var assemblyDirectory in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                var directoryName = Path.GetDirectoryName(assemblyDirectory);
                var pluginDLLLocation = Directory.EnumerateFiles(assemblyDirectory, $"{directoryName}.dll", SearchOption.AllDirectories).FirstOrDefault();

                if (pluginDLLLocation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Loading Assembly: {pluginDLLLocation}");
                    PluginLoadContext loadContext = new PluginLoadContext(pluginDLLLocation);
                    var pluginAssembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginDLLLocation)));
                }
            }

            return scan;
        }
        #endregion

        #region SPA XSRF
        /// <summary>
        /// Uses SPA XSRF Token Middleware.
        /// </summary>
        public static IApplicationBuilder UseSpaGenerateAntiforgeryTokenMiddleware(this IApplicationBuilder builder, Action<SpaGenerateAntiforgeryTokenOptions> configureOptions = null)
        {
            var options = new SpaGenerateAntiforgeryTokenOptions();
            if (configureOptions != null)
                configureOptions(options);

            return builder.UseMiddleware<SpaGenerateAntiforgeryTokenOptions>(options);
        }
        #endregion

        #region GraphQL

        public static IEndpointConventionBuilder MapGraphQLSchema<TSchema>(this IEndpointRouteBuilder endpoints, string path = "/graphql") where TSchema : ISchema
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseWebSockets().UseGraphQL<TSchema>(path).Build();
            return endpoints.Map(path, requestHandler);
        }

        public static IEndpointConventionBuilder MapGraphQLPlayground(this IEndpointRouteBuilder endpoints, Action<GraphQLPlaygroundOptions> configAction = null)
        {
            var options = new GraphQLPlaygroundOptions();
            if (configAction != null)
                configAction(options);

            var requestHandler = endpoints.CreateApplicationBuilder().UseWebSockets().UseGraphQLPlayground(options).Build();
            return endpoints.Map((options.Path != null ? options.Path.Value : "/ui/playground") + "/{**path}", requestHandler);
        }

        public static IEndpointConventionBuilder MapGraphQLVoyager(this IEndpointRouteBuilder endpoints, Action<GraphQLVoyagerOptions> configAction = null)
        {
            var options = new GraphQLVoyagerOptions();
            if (configAction != null)
                configAction(options);

            var requestHandler = endpoints.CreateApplicationBuilder().UseWebSockets().UseGraphQLVoyager(options).Build();
            return endpoints.Map((options.Path != null ? options.Path.Value : "/ui/voyager") + "/{**path}", requestHandler);
        }
        #endregion

        #region User Service
        public static IServiceCollection AddUserService(this IServiceCollection services)
        {
            return services.AddScoped<IUserService, UserService>();
        }

        #endregion
    }
}