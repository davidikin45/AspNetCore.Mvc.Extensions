using AspNetCore.Mvc.Extensions.Middleware.ImageProcessing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;

namespace AspNetCore.Mvc.Extensions.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseContentHandler(
           this IApplicationBuilder builder, IWebHostEnvironment env, Action<ContentHttpHandlerOptions> configureOptions = null)
        {
            var options = new ContentHttpHandlerOptions();
            if (configureOptions != null)
                configureOptions(options);

            return builder.UseMiddleware<ContentHandlerMiddleware>(options, env);
        }

        public static IApplicationBuilder UseResponseCachingCustom(
           this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseCachingCustomMiddleware>();
        }

        public static IApplicationBuilder UseVersionedStaticFiles(
         this IApplicationBuilder app, int days)
        {
            return app.UseWhen(context => context.Request.Query.ContainsKey("v"),
                   appBranch =>
                   {
                      //cache js, css
                      appBranch.UseStaticFiles(new StaticFileOptions
                       {
                           OnPrepareResponse = ctx =>
                           {
                               if (days > 0)
                               {
                                   TimeSpan timeSpan = new TimeSpan(days * 24, 0, 0);
                                   ctx.Context.Response.GetTypedHeaders().Expires = DateTime.Now.Add(timeSpan).Date.ToUniversalTime();
                                   ctx.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                                   {
                                       Public = true,
                                       MaxAge = timeSpan
                                   };
                                   //ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                               }
                               else
                               {
                                   ctx.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                                   {
                                       NoCache = true
                                   };
                                  //ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                               }
                           }
                       });
                   }
              );
        }

        public static IApplicationBuilder UseNonVersionedStaticFiles(
       this IApplicationBuilder app, int days)
        {
            return app.UseWhen(context => !context.Request.Query.ContainsKey("v"),
                   appBranch =>
                   {
                      //cache js, css
                      appBranch.UseStaticFiles(new StaticFileOptions
                       {
                           OnPrepareResponse = ctx =>
                           {
                               if (days > 0)
                               {
                                   TimeSpan timeSpan = new TimeSpan(days * 24, 0, 0);
                                   ctx.Context.Response.GetTypedHeaders().Expires = DateTime.Now.Add(timeSpan).Date.ToUniversalTime();
                                   ctx.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                                   {
                                       Public = true,
                                       MaxAge = timeSpan
                                   };
                                   //ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                               }
                               else
                               {
                                   ctx.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                                   {
                                       NoCache = true
                                   };
                                   //ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                               }
                           }
                       });
                   }
              );
        }

        public static IApplicationBuilder UseStackifyPrefix(this IApplicationBuilder app)
        {
            return app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
        }
    }
}
