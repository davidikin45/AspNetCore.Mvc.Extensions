using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Routing
{
    //.NET Core 3.0
    //https://dzone.com/articles/implement-middlewares-using-endpoint-routing-in-as
    //public static class EndpointRoutingExtensionMethods
    //{
    //    public static IEndpointConventionBuilder MapMiddlewareRoute(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action)
    //    {
    //        var nested = endpoints.CreateApplicationBuilder();
    //        action(nested);
    //        return endpoints.Map(pattern, nested.Build());
    //    }

    //    public static IEndpointConventionBuilder MapMiddlewareGet(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action)
    //    {
    //        var nested = endpoints.CreateApplicationBuilder();
    //        action(nested);
    //        return endpoints.MapGet(pattern, nested.Build());
    //    }
    //    public static IEndpointConventionBuilder MapMiddlewarePost(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action)
    //    {
    //        var nested = endpoints.CreateApplicationBuilder();
    //        action(nested);
    //        return endpoints.MapPost(pattern, nested.Build());
    //    }
    //    public static IEndpointConventionBuilder MapMiddlewareDelete(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action)
    //    {
    //        var nested = endpoints.CreateApplicationBuilder();
    //        action(nested);
    //        return endpoints.MapDelete(pattern, nested.Build());
    //    }

    //    public static IEndpointConventionBuilder MapMiddlewarePut(this IEndpointRouteBuilder endpoints, string pattern, Action<IApplicationBuilder> action)
    //    {
    //        var nested = endpoints.CreateApplicationBuilder();
    //        action(nested);
    //        return endpoints.MapPut(pattern, nested.Build());
    //    }
    //    public static IEndpointConventionBuilder MapMiddlewareMethods(this IEndpointRouteBuilder endpoints, string pattern, IEnumerable<string> methods, Action<IApplicationBuilder> action)
    //    {
    //        var nested = endpoints.CreateApplicationBuilder();
    //        action(nested);
    //        return endpoints.MapMethods(pattern, methods, nested.Build());
    //    }

    //    public static IEndpointConventionBuilder MapHandlerMiddlewareRoute<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, params object[] args)
    //    {
    //        var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
    //        return endpoints.Map(pattern, requestHandler);
    //    }

    //    public static IEndpointConventionBuilder MapHandlerMiddlewareGet<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, params object[] args)
    //    {
    //        var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
    //        return endpoints.MapGet(pattern, requestHandler);
    //    }

    //    public static IEndpointConventionBuilder MapHandlerMiddlewarePost<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, params object[] args)
    //    {
    //        var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
    //        return endpoints.MapPost(pattern, requestHandler);
    //    }

    //    public static IEndpointConventionBuilder MapHandlerMiddlewarePut<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, params object[] args)
    //    {
    //        var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
    //        return endpoints.MapPut(pattern, requestHandler);
    //    }

    //    public static IEndpointConventionBuilder MapHandlerMiddlewareDelete<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, params object[] args)
    //    {
    //        var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
    //        return endpoints.MapDelete(pattern, requestHandler);
    //    }

    //    public static IEndpointConventionBuilder MapHandlerMiddlewareMethods<Middleware>(this IEndpointRouteBuilder endpoints, string pattern, IEnumerable<string> methods, params object[] args)
    //    {
    //        var requestHandler = endpoints.CreateApplicationBuilder().UseMiddleware<Middleware>(args).Build();
    //        return endpoints.MapMethods(pattern, methods, requestHandler);
    //    }
    //}
}
