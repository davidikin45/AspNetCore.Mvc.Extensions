using Microsoft.OpenApi.Models;
using System;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    public class SwaggerVersioningOptions
    {
        public string SwaggerUIRoutePrefix
        {
            get; set;
        } = "api";

        public string UIUsername { get; set; }
        public string UIPassword { get; set; }

        public string ApiTitle { get; set; } = $"{Assembly.GetExecutingAssembly().GetName().Name} API";
        public string ApiDescription { get; set; } = "";
        public string XmlCommentsFullPath { get; set; } =  System.IO.Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

        public string ContactName { get; set; } = "";
        public string ContactWebsite { get; set; }
        public string ContactEmail { get; set; }

        public OpenApiLicense License { get; set; } = new OpenApiLicense() { Name = "MIT LICENSE", Url = new System.Uri("https://opensource.org/licenses/MIT") };
}
}
