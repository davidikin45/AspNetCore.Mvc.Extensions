using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authentication
{
    public interface IOpenIDConnectService
    {
        //Client Credentials
        Task<(string AccessToken, DateTime Expiry)> GetApiAccessTokenAsync();

        //Authorization Code Flow (PKCE)
        Task<(string AuthorizeUrl, string CodeVerifier)> CreateAuthorizeUrlAsync(HttpContext context);
        Task<string> CreateLogoutUrlAsync(HttpContext context, string idTokenHint);
        Task<(string AccessToken, string IdentityToken, DateTime Expiry)> GetApiAccessTokenAsync(HttpContext context, string code, string codeVerifier);

        Task<IEnumerable<Claim>> GetUserInfoAsync(string accessToken);

        Task RevokeAccessAsync(string accessToken, string refreshToken = null);

        Task<(string AccessToken, DateTime Expiry, string RefreshToken, string IdentityToken)> RenewTokensAsync(string refreshToken);
    }
}
