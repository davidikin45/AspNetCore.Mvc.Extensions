using AspNetCore.Mvc.Extensions.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
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
            IEnumerable<AuthorizeAttribute> authorizeAttributes = new List<AuthorizeAttribute>();

            // Determine if the operation has the Authorize attribute
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor)
            {
                authorizeAttributes = ((ControllerActionDescriptor)context.ApiDescription.ActionDescriptor).MethodInfo.ReflectedType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Select(a => (AuthorizeAttribute)a);
            }

            authorizeAttributes = authorizeAttributes.Concat(context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Select(a => (AuthorizeAttribute)a));

            if (!authorizeAttributes.Any())
                return;

            // Initialize the operation.security property
            if (operation.Security == null)
                operation.Security = new List<OpenApiSecurityRequirement>();

            // Add the appropriate security definition to the operation
            var securityRequirements = new OpenApiSecurityRequirement();

            foreach (var item in authorizeAttributes)
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

            if (securityRequirements.Count() == 0)
            {
                securityRequirements.Add(SwaggerSecuritySchemes.CookiesReference, new List<string>());
            }

            operation.Security.Add(securityRequirements);
        }
    }
}
