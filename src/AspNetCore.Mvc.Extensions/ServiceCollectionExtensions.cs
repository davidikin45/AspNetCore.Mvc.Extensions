using AspNetCore.Mvc.Extensions.Conventions.Display;
using AspNetCore.Mvc.Extensions.FluentMetadata;
using AspNetCore.Mvc.Extensions.Providers;
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
            return services.AddSingleton<IHtmlGenerator, ConventionsHtmlGenerator>();
        }

        public static IServiceCollection AddMvcDisplayConventions(this IServiceCollection services, params IDisplayConventionFilter[] displayConventions)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new DisplayConventionsMetadataProvider(displayConventions));
            });

            if (displayConventions.OfType<AppendAsterixToRequiredFieldLabels>().Any())
            {
                services.AddAppendAsterixToRequiredFieldLabels();
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
            return services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new AttributeMetadataProvider());
            });
        }

        public static IServiceCollection AddInheritanceValidationAttributeAdapterProvider(this IServiceCollection services)
        {
            services.RemoveAll<IValidationAttributeAdapterProvider>();
            services.AddSingleton<IValidationAttributeAdapterProvider, InheritanceValidationAttributeAdapterProvider>();

            return services;
        }

        public static IServiceCollection AddFluentMetadata(this IServiceCollection services, Action<AssemblyProviderOptions> configureOptions = null)
        {
            configureOptions = configureOptions ?? ((options) => { });

            services.Configure(configureOptions);

            services.AddTransient(sp => sp.GetService<IOptions<AssemblyProviderOptions>>().Value);

            services.AddSingleton<IAssemblyProvider, AssemblyProvider>();
            services.AddSingleton<ITypeFinder, TypeFinder>();
            services.AddSingleton<IMetadataConfiguratorProviderSingleton, MetadataConfiguratorProviderSingleton>();
            services.AddSingleton<IConfigureOptions<MvcOptions>, FluentMetadataConfigureMvcOptions>();

            return services;
        }
    }
}
