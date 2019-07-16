using AspNetCore.Mvc.Extensions.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    public static class SwaggerConfigurationServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerWithApiVersioning(this IServiceCollection services, Action<SwaggerVersioningOptions> setupAction = null)
        {
            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton(sp => sp.GetService<IOptions<SwaggerVersioningOptions>>().Value);

            //using Swashbuckle.AspNetCore v5
            services.AddSwaggerGen();
            services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigOptions>();
            return services;
        }
    }

    public class SwaggerConfigOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
        private readonly SwaggerVersioningOptions _options;

        public SwaggerConfigOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider, IOptions<SwaggerVersioningOptions> options)
        {
            _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
            _options = options.Value;
        }

        public void Configure(SwaggerGenOptions c)
        {
            foreach (var apiDescription in _apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                c.SwaggerDoc(apiDescription.GroupName, new OpenApiInfo { Title = _options.ApiTitle, Description = _options.ApiDescription, Contact = new OpenApiContact() { Name = _options.ContactName, Email = _options.ContactEmail, Url = _options.ContactWebsite != null ?  new System.Uri(_options.ContactWebsite) : null }, Version = apiDescription.ApiVersion.ToString(), License = _options.License });
            }

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, SwaggerSecuritySchemes.Bearer);

            c.AddSecurityDefinition(BasicAuthenticationDefaults.AuthenticationScheme, SwaggerSecuritySchemes.Basic);

            //c.AddSecurityDefinition(CookieAuthenticationDefaults.AuthenticationScheme, SwaggerSecuritySchemes.Cookies);

            c.DocInclusionPredicate((documentName, apiDescription) =>
            {
                var actionApiVersionModel = apiDescription.ActionDescriptor
                .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                if (actionApiVersionModel == null)
                {
                    return true;
                }

                if (actionApiVersionModel.DeclaredApiVersions.Any())
                {
                    return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                    $"v{v.ToString()}" == documentName);
                }

                return actionApiVersionModel.ImplementedApiVersions.Any(v =>
                    $"v{v.ToString()}" == documentName);
            });

            //Accept Header Operation Filter
            c.OperationFilter<SwaggerAssignSecurityRequirements>();


            c.SchemaFilter<SwaggerModelExamples>();

            c.IncludeXmlComments(_options.XmlCommentsFullPath);
            c.DescribeAllEnumsAsStrings();

            c.DescribeAllParametersInCamelCase();
        }
    }
}
