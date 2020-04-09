using AspNetCore.Mvc.Extensions.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    public class SwaggerAssignSecurityRequirements : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //AllowAnonymous at Controller or Action level always takes priority!
            var allowAnonymous = context.MethodInfo.ReflectedType.GetCustomAttributes(true)
           .Union(context.MethodInfo.GetCustomAttributes(true))
           .OfType<AllowAnonymousAttribute>().Any();

            //https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            //AuthorizeAttributes are AND not OR.
            var authAttributes = context.MethodInfo.ReflectedType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

            if (allowAnonymous || !authAttributes.Any())
                return;

            // Initialize the operation.security property
            if (operation.Security == null)
                operation.Security = new List<OpenApiSecurityRequirement>();

            // Add the appropriate security definition to the operation
            var securityRequirements = new OpenApiSecurityRequirement();

            //If no scheme is specified any scheme can be used.
            foreach (var item in authAttributes)
            {
                if (item.AuthenticationSchemes == null || item.AuthenticationSchemes.Contains(JwtBearerDefaults.AuthenticationScheme))
                {
                    if (!securityRequirements.ContainsKey(SwaggerSecuritySchemes.BearerReference))
                    {
                        securityRequirements.Add(SwaggerSecuritySchemes.BearerReference, new List<string>());
                    }
                }
                if (item.AuthenticationSchemes == null || item.AuthenticationSchemes.Contains(CookieAuthenticationDefaults.AuthenticationScheme))
                {
                    if (!securityRequirements.ContainsKey(SwaggerSecuritySchemes.CookiesReference))
                    {
                        securityRequirements.Add(SwaggerSecuritySchemes.CookiesReference, new List<string>());
                    }
                }
                if (item.AuthenticationSchemes == null || item.AuthenticationSchemes.Contains(BasicAuthenticationDefaults.AuthenticationScheme))
                {
                    if (!securityRequirements.ContainsKey(SwaggerSecuritySchemes.BasicReference))
                    {
                        securityRequirements.Add(SwaggerSecuritySchemes.BasicReference, new List<string>());
                    }
                }
            }

            operation.Security.Add(securityRequirements);
        }
    }
}
