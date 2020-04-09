using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Routing
{
    //.NET Core 3.0
    //https://dzone.com/articles/implement-middlewares-using-endpoint-routing-in-as
    public static class EndpointRoutingExtensionMethods
    {
        public static IEndpointConventionBuilder MapMiddlewareRoute(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action, int order = 0)
        {
            var nested = endpoints.CreateApplicationBuilder();
            action(nested);
            var conventionBuilder = endpoints.Map(pattern, nested.Build());
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapMiddlewareGet(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action, int order = 0)
        {
            var nested = endpoints.CreateApplicationBuilder();
            action(nested);
            var conventionBuilder = endpoints.MapGet(pattern, nested.Build());
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }
        public static IEndpointConventionBuilder MapMiddlewarePost(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action, int order = 0)
        {
            var nested = endpoints.CreateApplicationBuilder();
            action(nested);
            var conventionBuilder = endpoints.MapPost(pattern, nested.Build());
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }
        public static IEndpointConventionBuilder MapMiddlewareDelete(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action, int order = 0)
        {
            var nested = endpoints.CreateApplicationBuilder();
            action(nested);
            var conventionBuilder = endpoints.MapDelete(pattern, nested.Build());
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapMiddlewarePut(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action, int order = 0)
        {
            var nested = endpoints.CreateApplicationBuilder();
            action(nested);
            var conventionBuilder = endpoints.MapPut(pattern, nested.Build());
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }
        public static IEndpointConventionBuilder MapMiddlewareMethods(this IEndpointRouteBuilder endpoints, string pattern, IEnumerable<string> methods, Action<IApplicationBuilder> action, int order = 0)
        {
            var nested = endpoints.CreateApplicationBuilder();
            action(nested);
            var conventionBuilder = endpoints.MapMethods(pattern, methods, nested.Build());
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapHandlerMiddlewareRoute<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, int order = 0, params object[] args)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
            var conventionBuilder = endpoints.Map(pattern, requestHandler);
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapHandlerMiddlewareGet<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, int order = 0, params object[] args)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
            var conventionBuilder = endpoints.MapGet(pattern, requestHandler);
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapHandlerMiddlewarePost<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, int order = 0, params object[] args)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
            var conventionBuilder = endpoints.MapPost(pattern, requestHandler);
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapHandlerMiddlewarePut<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, int order = 0, params object[] args)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
            var conventionBuilder = endpoints.MapPut(pattern, requestHandler);
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapHandlerMiddlewareDelete<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, int order = 0, params object[] args)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
            var conventionBuilder = endpoints.MapDelete(pattern, requestHandler);
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }

        public static IEndpointConventionBuilder MapHandlerMiddlewareMethods<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, IEnumerable<string> methods, int order = 0, params object[] args)
        {
            var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
            var conventionBuilder = endpoints.MapMethods(pattern, methods, requestHandler);
            conventionBuilder.Add(b => ((RouteEndpointBuilder)b).Order = order);
            return conventionBuilder;
        }
    }
}
