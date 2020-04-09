using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Security
{
    public static class JWTInHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }

    //Cookie-based authentication
    //When a user authenticates using their username and password, they're issued a token, containing an authentication ticket that can be used for authentication and authorization. The token is stored as a cookie that accompanies every request the client makes. Generating and validating this cookie is performed by the Cookie Authentication Middleware. The middleware serializes a user principal into an encrypted cookie. On subsequent requests, the middleware validates the cookie, recreates the principal, and assigns the principal to the User property of HttpContext.

    //Token-based authentication
    //When a user is authenticated, they're issued a token (not an antiforgery token). The token contains user information in the form of claims or a reference token that points the app to user state maintained in the app. When a user attempts to access a resource requiring authentication, the token is sent to the app with an additional authorization header in form of Bearer token. This makes the app stateless. In each subsequent request, the token is passed in the request for server-side validation. This token isn't encrypted; it's encoded. On the server, the token is decoded to access its information. To send the token on subsequent requests, store the token in the browser's local storage.Don't be concerned about CSRF vulnerability if the token is stored in the browser's local storage.CSRF is a concern when the token is stored in a cookie.

    //CSRF attacks are possible against web apps that use cookies for authentication because:
    //Browsers store cookies issued by a web app.
    //Stored cookies include session cookies for authenticated users.
    //Browsers send all of the cookies associated with a domain to the web app every request regardless of how the request to app was generated within the browser.

    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var accessTokenCookie = context.Request.Cookies["access_token"];
            if (accessTokenCookie != null)
            {
                context.Request.Headers.Remove("Authorization");
                context.Request.Headers.Append("Authorization", $"Bearer {accessTokenCookie}");
            }

            return _next.Invoke(context);
        }
    }
}
