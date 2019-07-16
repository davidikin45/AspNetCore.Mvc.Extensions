using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    public static class SwaggerSecuritySchemes
    {
        public static OpenApiSecurityScheme Basic => new OpenApiSecurityScheme
        {
            Description = "Input your username and password to access this API",
            Name = "Authorization",
            Scheme = "basic",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http
        };

        public static OpenApiSecurityScheme BasicReference => new OpenApiSecurityScheme()
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Basic",
            }
        };

        public static OpenApiSecurityScheme Bearer => new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            Scheme = "bearer",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http
        };

        public static OpenApiSecurityScheme BearerReference => new OpenApiSecurityScheme()
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
            }
        };

        //OpenIdConnect
        public static OpenApiSecurityScheme Cookies => new OpenApiSecurityScheme
        {
            Description = "OpenIdConnect Cookie Authorization.",
            Name = "CookieName",
            In = ParameterLocation.Cookie,
            Type = SecuritySchemeType.ApiKey
        };

        public static OpenApiSecurityScheme CookiesReference => new OpenApiSecurityScheme()
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Cookies",
            }
        };
    }
}
