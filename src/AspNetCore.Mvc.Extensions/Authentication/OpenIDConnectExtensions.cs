using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authentication
{
    public static class OpenIDConnectExtensions
    {
        public static IEnumerable<Claim> OIDCClaims(this HttpContext context)
        {
            return context.User.Claims;
        }

        public async static Task OIDCSignOutOfLocalAndIDPAsync(this HttpContext context)
        {
            await OIDCRevokeAccessAsync(context);
            await OIDCSignOutOfLocalAsync(context);
            await OIDCSignOutIDPAsync(context);
        }

        public static Task OIDCSignOutOfLocalAsync(this HttpContext context)
        {
            return context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public static Task OIDCSignOutIDPAsync(this HttpContext context)
        {
            return context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);            
        }

        public static Task<string> OIDCGetAccessTokenAsync(this HttpContext context)
        {
            return context.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
        }

        public async static Task OIDCRevokeAccessAsync(this HttpContext context)
        {
            var access_token = await OIDCGetAccessTokenAsync(context);
            var refresh_token = await OIDCGetRefreshTokenAsync(context);
            var client = context.RequestServices.GetRequiredService<IOpenIDConnectService>();
            await client.RevokeAccessAsync(access_token, refresh_token);
        }

        public static Task<string> OIDCGetRefreshTokenAsync(this HttpContext context)
        {
            return context.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
        }

        public static async Task<Nullable<DateTime>> OIDCGetTokenExpiryAsync(this HttpContext context)
        {
            var value = await context.GetTokenAsync("expires_at");
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            else
            {
                return DateTime.Parse(value).ToUniversalTime();
            }
        }
    }
}
