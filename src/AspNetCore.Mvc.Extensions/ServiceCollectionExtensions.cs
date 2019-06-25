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
    public static class ServiceCollectionExtensions
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

        public static IServiceCollection AddMvcDisplayConventions(this IServiceCollection services, params IDisplayConventionFilter[] displayConventions)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new DisplayConventionsMetadataProvider(displayConventions));
            });

            if (displayConventions.OfType<AppendAsterixToRequiredFieldLabels>().Any())
            {
                var addAsterix = displayConventions.OfType<AppendAsterixToRequiredFieldLabels>().FirstOrDefault().ApplyConvention;
                if (addAsterix != null)
                {
                    services.AddAppendAsterixToRequiredFieldLabels(options => options.AddAstertix = addAsterix);
                }
                else
                {
                    services.AddAppendAsterixToRequiredFieldLabels();
                }
            }

            return services;
        }

        public static IServiceCollection AddMvcValidationConventions(this IServiceCollection services, params IValidationConventionFilter[] validationConventions)
        {
            return services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new ValidationConventionsMetadataProvider(validationConventions));
            });
        }

        public static IServiceCollection AddMvcDisplayAttributes(this IServiceCollection services)
        {
            return services.AddSingleton<IConfigureOptions<MvcOptions>, AttributeMetadataProviderSetup>();
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

        public static IServiceCollection AddInheritanceValidationAttributeAdapterProvider(this IServiceCollection services)
        {
            services.RemoveAll<IValidationAttributeAdapterProvider>();
            services.AddSingleton<IValidationAttributeAdapterProvider, InheritanceValidationAttributeAdapterProvider>();

            return services;
        }

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

        public static IServiceCollection AddViewRenderer(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            return services.AddSingleton<IViewRenderService, ViewRenderService>();
        }
    }
}
