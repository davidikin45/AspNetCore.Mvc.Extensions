using AspNetCore.Mvc.Extensions.Conventions.Display;
using AspNetCore.Mvc.Extensions.FluentMetadata;
using AspNetCore.Mvc.Extensions.Providers;
using AspNetCore.Mvc.Extensions.Razor;
using AspNetCore.Mvc.Extensions.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
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
        public static IServiceCollection AddAppendAsterixToRequiredFieldLabels(this IServiceCollection services)
        {
            //Appends '*' to required fields
            services.AddTransient(sp => sp.GetService<IOptions<ConventionsHtmlGeneratorOptions>>().Value);
            return services.AddSingleton<IHtmlGenerator, ConventionsHtmlGenerator>();
        }

        public static IServiceCollection AddAppendAsterixToRequiredFieldLabels(this IServiceCollection services, Action<ConventionsHtmlGeneratorOptions> setupAction)
        {
            //Appends '*' to required fields
            services.AddAppendAsterixToRequiredFieldLabels();
            services.Configure(setupAction);
            return services;
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
                    services.AddAppendAsterixToRequiredFieldLabels(options => options.AddAstertix = addAsterix);
                }
                else
                {
                    services.AddAppendAsterixToRequiredFieldLabels();
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
    }
}
