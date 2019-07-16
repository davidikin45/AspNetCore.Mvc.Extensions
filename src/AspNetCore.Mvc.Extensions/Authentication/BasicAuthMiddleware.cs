using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authentication
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string realm;
        private readonly string username;
        private readonly string password;

        public BasicAuthMiddleware(RequestDelegate next, string realm, string username, string password)
        {
            this.next = next;
            this.realm = realm;
            this.username = username;
            this.password = password;
        }

        public async Task Invoke(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                // Get the encoded username and password
                var encodedUsernamePassword = authHeader.Split(' ')[1]?.Trim();
                // Decode from Base64 to string
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                // Split username and password
                var username = decodedUsernamePassword.Split(':')[0];
                var password = decodedUsernamePassword.Split(':')[1];
                // Check if login is correct
                if (IsAuthorized(username, password))
                {
                    await next.Invoke(context);
                    return;
                }
            }
            // Return authentication type (causes browser to show login dialog)
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            // Add realm if it is not null
            if (!string.IsNullOrWhiteSpace(realm))
            {
                context.Response.Headers["WWW-Authenticate"] += $" realm=\"{realm}\"";
            }
            // Return unauthorized
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        // Make your own implementation of this
        public bool IsAuthorized(string username, string password)
        {
            // Check that username and password are correct
            return username.Equals(this.username, StringComparison.InvariantCultureIgnoreCase)
                   && password.Equals(this.password);
        }
    }
}