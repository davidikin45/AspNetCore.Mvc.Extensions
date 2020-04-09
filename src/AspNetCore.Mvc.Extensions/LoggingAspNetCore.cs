using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    //Serilog.AspNetCore
    //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-reducing-log-verbosity/
    //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-the-selected-endpoint-name-with-serilog/
    //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-mvc-propertis-with-serilog/
    //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-excluding-health-check-endpoints-from-serilog-request-logging/
    //app.UseSerilogRequestLogging(options => {
    //options.EnrichDiagnosticContext = LoggingAspNetCore.EnrichFromRequest;
    //options.GetLevel = LoggingAspNetCore.ExcludeHealthChecks; // Use the custom level
    //);
    public static class LoggingAspNetCore
    {
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;

            // Set all the common properties available for every request
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress.ToString());
            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);

            // Only set it if available. You're not sending sensitive data in a querystring right?!
            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            //diagnosticContext.Set("Query", httpContext.Request.Query.ToDictionary(x => x.Key, y => y.Value.ToString()), true);
            //diagnosticContext.Set("Headers", httpContext.Request.Headers.ToDictionary(x => x.Key, y => y.Value.ToString()), true);
            //diagnosticContext.Set("Cookies", httpContext.Request.Cookies.ToDictionary(x => x.Key, y => y.Value.ToString()), true);

            // Set the content-type of the Response at this point
            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            diagnosticContext.Set("UserInfo", GetUserInfo(httpContext.User), true);


            // Retrieve the IEndpointFeature selected for the request
            var endpoint = httpContext.GetEndpoint();
            if (endpoint is object) // endpoint != null
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }
        private class UserInformation
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public Dictionary<string, List<string>> UserClaims { get; set; }
        }

        private static UserInformation GetUserInfo(ClaimsPrincipal ctxUser)
        {
            var user = ctxUser.Identity;
            if (user?.IsAuthenticated != true) return null;

            var excludedClaims = new List<string>
            { "nbf", "exp", "auth_time", "amr", "sub", "at_hash",
                "s_hash", "sid", "name", "preferred_username" };

            const string userIdClaimType = "sub";
            const string userNameClaimType = "name";

            var userInfo = new UserInformation
            {
                UserId = ctxUser.Claims.FirstOrDefault(a => a.Type == userIdClaimType)?.Value,
                UserName = ctxUser.Claims.FirstOrDefault(a => a.Type == userNameClaimType)?.Value,
                UserClaims = new Dictionary<string, List<string>>()
            };
            foreach (var distinctClaimType in ctxUser.Claims
                .Where(a => excludedClaims.All(ex => ex != a.Type))
                .Select(a => a.Type)
                .Distinct())
            {
                userInfo.UserClaims[distinctClaimType] = ctxUser.Claims
                    .Where(a => a.Type == distinctClaimType)
                    .Select(c => c.Value)
                    .ToList();
            }

            return userInfo;
        }

        private static bool IsHealthCheckEndpoint(HttpContext ctx)
        {

            var endpoint = ctx.GetEndpoint();
            if (endpoint is object) // same as !(endpoint is null)
            {
                return string.Equals(
                    endpoint.DisplayName,
                    "Health checks",
                    StringComparison.Ordinal);
            }

            // No endpoint, so not a health check endpoint
            return false;
        }

        public static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
      ex != null
          ? LogEventLevel.Error
          : ctx.Response.StatusCode > 499
              ? LogEventLevel.Error
              : IsHealthCheckEndpoint(ctx) // Not an error, check if it was a health check
                  ? LogEventLevel.Verbose // Was a health check, use Verbose
                  : LogEventLevel.Information;
    }
}
