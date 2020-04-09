using AspNetCore.Mvc.Extensions.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    //Spa > Server > API
    //Spa > API > API

    //As long as OpenIdConnectOptions.SaveTokens or JwtBearerOptions.SaveTokens or IdentityServerAuthenticationOptions.SaveTokens = true
    public class BearerRefreshHttpHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOpenIDConnectService _openIDConnectService;

        public BearerRefreshHttpHandler(IHttpContextAccessor httpContextAccessor, IOpenIDConnectService openIDConnectService)
        {
            _httpContextAccessor = httpContextAccessor ??
                 throw new ArgumentNullException(nameof(httpContextAccessor));
            _openIDConnectService = openIDConnectService ??
                   throw new ArgumentNullException(nameof(openIDConnectService));
        }

        public BearerRefreshHttpHandler(HttpMessageHandler innerHandler, IHttpContextAccessor httpContextAccessor, IOpenIDConnectService openIDConnectService)
            :base(innerHandler)
        {
            _httpContextAccessor = httpContextAccessor ??
                  throw new ArgumentNullException(nameof(httpContextAccessor));
            _openIDConnectService = openIDConnectService ??
                   throw new ArgumentNullException(nameof(openIDConnectService));
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var accessToken = await GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                //request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // get the expires_at value & parse it
            var expiresAt = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");

            var expiresAtAsDateTimeOffset = DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture);

            if ((expiresAtAsDateTimeOffset.AddSeconds(-60)).ToUniversalTime() > DateTime.UtcNow)
            {
                // no need to refresh, return the access token
                return await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            // refresh the tokens
            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var refreshResponse = await _openIDConnectService.RenewTokensAsync(refreshToken);

            // store the tokens             
            var updatedTokens = new List<AuthenticationToken>();
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.IdToken,
                Value = refreshResponse.IdentityToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = refreshResponse.AccessToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = refreshResponse.RefreshToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = "expires_at",
                Value = (refreshResponse.Expiry).ToString("o", CultureInfo.InvariantCulture)
            });

            // get authenticate result, containing the current principal & 
            // properties
            var currentAuthenticateResult = await _httpContextAccessor
                .HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // store the updated tokens
            currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

            // sign in
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                currentAuthenticateResult.Principal,
                currentAuthenticateResult.Properties);

            return refreshResponse.AccessToken;
        }
    }
}
