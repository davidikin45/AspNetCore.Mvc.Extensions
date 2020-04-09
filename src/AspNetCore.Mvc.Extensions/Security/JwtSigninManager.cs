using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Security
{
    public static class JwtSigninManager
    {
        public static void SignIn(HttpResponse response, string token)
        {
            var options = (IOptions<JwtBearerOptions>)response.HttpContext.RequestServices.GetService(typeof(IOptions<JwtBearerOptions>));

            SecurityToken validationToken = null;

            var user = new JwtSecurityTokenHandler().ValidateToken(token, options.Value.TokenValidationParameters, out validationToken);

            //Cookie-based authentication
            //When a user authenticates using their username and password, they're issued a token, containing an authentication ticket that can be used for authentication and authorization. The token is stored as a cookie that accompanies every request the client makes. Generating and validating this cookie is performed by the Cookie Authentication Middleware. The middleware serializes a user principal into an encrypted cookie. On subsequent requests, the middleware validates the cookie, recreates the principal, and assigns the principal to the User property of HttpContext.

            //Token-based authentication
            //When a user is authenticated, they're issued a token (not an antiforgery token). The token contains user information in the form of claims or a reference token that points the app to user state maintained in the app. When a user attempts to access a resource requiring authentication, the token is sent to the app with an additional authorization header in form of Bearer token. This makes the app stateless. In each subsequent request, the token is passed in the request for server-side validation. This token isn't encrypted; it's encoded. On the server, the token is decoded to access its information. To send the token on subsequent requests, store the token in the browser's local storage.Don't be concerned about CSRF vulnerability if the token is stored in the browser's local storage.CSRF is a concern when the token is stored in a cookie.

            //CSRF attacks are possible against web apps that use cookies for authentication because:
            //Browsers store cookies issued by a web app.
            //Stored cookies include session cookies for authenticated users.
            //Browsers send all of the cookies associated with a domain to the web app every request regardless of how the request to app was generated within the browser.

            response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions()
                {
                    //never accessible (both for reading or writing) from JavaScript running in the browser and are immune to XSS but not XSRF.
                    //https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-3.0
                    HttpOnly = true,
                    Expires = validationToken.ValidTo,
                    Path = "/"
                }
            );
        }

        public static void SignOut(HttpResponse response)
        {
            response.Cookies.Delete("access_token");
        }

        //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.2
        public static Task SigninAsync(this HttpContext httpContext, string token)
        {
            var options = (IOptions<JwtBearerOptions>)httpContext.RequestServices.GetService(typeof(IOptions<JwtBearerOptions>));

            SecurityToken validationToken = null;

            var user = new JwtSecurityTokenHandler().ValidateToken(token, options.Value.TokenValidationParameters, out validationToken);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                ExpiresUtc = validationToken.ValidTo,
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. Required when setting the 
                // ExpireTimeSpan option of CookieAuthenticationOptions 
                // set with AddCookie. Also required when setting 
                // ExpiresUtc.

                IssuedUtc = validationToken.ValidFrom,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            return httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                user,
                authProperties);
        }

        public static Task SignOutAsync(this HttpContext httpContext)
        {
            return httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
