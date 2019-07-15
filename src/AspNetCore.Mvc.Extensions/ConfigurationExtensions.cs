using AspNetCore.Mvc.Extensions.AmbientRouteData;
using AspNetCore.Mvc.Extensions.Conventions.Display;
using AspNetCore.Mvc.Extensions.FluentMetadata;
using AspNetCore.Mvc.Extensions.Internal;
using AspNetCore.Mvc.Extensions.NdjsonStream;
using AspNetCore.Mvc.Extensions.Providers;
using AspNetCore.Mvc.Extensions.Razor;
using AspNetCore.Mvc.Extensions.Reflection;
using AspNetCore.Mvc.Extensions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace AspNetCore.Mvc.Extensions
{
    public static class ConfigurationExtensions
    {
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

        /// <summary>
        /// Adds the fluent metadata services.
        /// </summary>
        public static IServiceCollection AddFluentMetadata(this IServiceCollection services)
        {
            services.AddTransient(sp => sp.GetService<IOptions<AssemblyProviderOptions>>().Value);

            services.AddSingleton<IAssemblyProvider, AssemblyProvider>();
            services.AddSingleton<ITypeFinder, TypeFinder>();
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

        /// <summary>
        /// Adds MVC feature services to the application.
        /// </summary>
        public static IMvcBuilder AddMvcFeatureService(this IMvcBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<FeatureService>();

            return builder;
        }

        /// <summary>
        /// Adds bundleconfig service to the application.
        /// </summary>
        public static IServiceCollection AddBundleConfigService(this IServiceCollection services)
        {
            return services.AddSingleton<BundleConfigService>();
        }

        /// <summary>
        /// Adds MVC json navigation service to the application.
        /// </summary>
        public static IMvcBuilder AddMvcJsonNavigationService(this IMvcBuilder builder, Action<JsonNavigationServiceOptions> setupAction = null)
        {
            var services = builder.Services;

            services.AddSingleton<JsonNavigationService>();

            if(setupAction != null)
                services.Configure(setupAction);

            return builder;
        }

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
    }
}
