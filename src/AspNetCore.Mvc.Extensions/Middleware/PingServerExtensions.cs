using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace AspNetCore.Mvc.Extensions.Middleware
{
    public static class PingServerExtension
    {

        public static IApplicationBuilder UsePing(this IApplicationBuilder app, string path)
        {
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            app.UseWhen(context => context.Request.Path.ToString().StartsWith("/ping"),
               appBranch =>
               {
                   appBranch.Run(async (context) =>
                   {
                       context.Response.ContentType = "text/html";
                       await context.Response
                           .WriteAsync("<!DOCTYPE html><html lang=\"en\"><head>" +
                               "<title></title></head><body><p>Hosted by Kestrel</p>");

                       if (serverAddressesFeature != null)
                       {
                           await context.Response
                               .WriteAsync("<p>Listening on the following addresses: " + string.Join(", ", serverAddressesFeature.Addresses) + "</p>");
                       }

                       await context.Response.WriteAsync("<p>Request URL: " + $"{context.Request.GetDisplayUrl()}<p>");

                       await context.Response
                           .WriteAsync("</body></html>");
                   });
               }
            );

            return app;
        }
    }
}
