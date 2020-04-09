using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authentication
{
    //https://auth0.com/docs/tokens
    //The ID Token is a security token granted by the OpenID Provider that contains information about an End-User. ... Access tokens, on the other hand, are not intended to carry information about the user.They simply allow access to certain defined server resources.
    public class OpenIDConnectService : IOpenIDConnectService
    {
        private string clientId;
        private string clientSecret;
        private PathString signedInCallbackPath;
        private PathString signedOutCallbackPath;

        private string responseType;

        private string scopes;

        private string userInfoEndpoint;
        private string tokenEndpoint;
        private string revocationEndpoint;
        private string authorizeEndpoint;

        private bool endPointsLoaded;

        private readonly HttpClient _httpClient;

        public OpenIDConnectService(HttpClient httpClient, string clientId, string responseType, string scopes, string signedInCallbackPath = null, string signedOutCallbackPath = null, string clientSecret = null)
        {
            _httpClient = httpClient;
            this.clientId = clientId;
            this.responseType = responseType;
            this.scopes = scopes;
            this.clientSecret = clientSecret;
            this.signedInCallbackPath = signedInCallbackPath ?? new PathString("/signin-oidc");
            this.signedOutCallbackPath = signedOutCallbackPath ?? new PathString("/signout-callback-oidc");
        }

        private async Task LoadEndpointsAsync()
        {
            if (!endPointsLoaded)
            {
                var metaDataResponse = await _httpClient.GetDiscoveryDocumentAsync();
                userInfoEndpoint = metaDataResponse.UserInfoEndpoint;
                tokenEndpoint = metaDataResponse.TokenEndpoint;
                revocationEndpoint = metaDataResponse.RevocationEndpoint;
                authorizeEndpoint = metaDataResponse.AuthorizeEndpoint;

                endPointsLoaded = true;
            }
        }

        public async Task<(string AccessToken, DateTime Expiry)> GetApiAccessTokenAsync()
        {
            await LoadEndpointsAsync();

            var response = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scopes
            });

            if (response.IsError)
            {
                throw new Exception("Problem accessing the Token endpoint.", response.Exception);
            }

            var expiry = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(response.ExpiresIn);

            return (response.AccessToken, expiry);
        }

        //https://auth0.com/docs/flows/concepts/auth-code-pkce
        public async Task<(string AuthorizeUrl, string CodeVerifier)> CreateAuthorizeUrlAsync(HttpContext context)
        {
            await LoadEndpointsAsync();

            var originalPathBase = context.Features.Get<IAuthenticationFeature>()?.OriginalPathBase ?? context.Request.PathBase;
            var redirectUri = context.Request.Scheme + "://" + context.Request.Host + originalPathBase + signedInCallbackPath;

            var ru = new RequestUrl(authorizeEndpoint);

            var pkce = GeneratePKCEValues();

            var url = ru.CreateAuthorizeUrl(
                clientId: clientId,
                responseType: responseType,
                redirectUri: redirectUri,
                nonce: CryptoRandom.CreateUniqueId(32),
                codeChallengeMethod: pkce.CodeChallengeMethod,
                codeChallenge: pkce.CodeChallenge,
                scope: scopes);

            return (url, pkce.CodeVerifier);
        }

        public async Task<string> CreateLogoutUrlAsync(HttpContext context, string idTokenHint)
        {
            await LoadEndpointsAsync();

            var originalPathBase = context.Features.Get<IAuthenticationFeature>()?.OriginalPathBase ?? context.Request.PathBase;
            var postLogoutRedirectUri = context.Request.Scheme + "://" + context.Request.Host + originalPathBase + signedOutCallbackPath;

            var ru = new RequestUrl(authorizeEndpoint);

            var pkce = GeneratePKCEValues();

            var url = ru.CreateEndSessionUrl(
                idTokenHint: idTokenHint,
                postLogoutRedirectUri: postLogoutRedirectUri);

            return url;
        }

        private (string CodeVerifier, string CodeChallengeMethod, string CodeChallenge) GeneratePKCEValues()
        {
            // generate code_verifier
            var codeVerifier = CryptoRandom.CreateUniqueId(32);

            // create code_challenge
            string codeChallenge;
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                codeChallenge = Base64Url.Encode(challengeBytes);
            }

            return (codeVerifier, "S256", codeChallenge);
        }

        public async Task<(string AccessToken, string IdentityToken, DateTime Expiry)> GetApiAccessTokenAsync(HttpContext context, string code, string codeVerifier)
        {
            await LoadEndpointsAsync();

            var originalPathBase = context.Features.Get<IAuthenticationFeature>()?.OriginalPathBase ?? context.Request.PathBase;
            var redirectUri = context.Request.Scheme + "://" + context.Request.Host + originalPathBase + signedInCallbackPath;

            var response = await _httpClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest()
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Code = code,
                RedirectUri = redirectUri,
                CodeVerifier = codeVerifier
            });

            if (response.IsError)
            {
                throw new Exception("Problem accessing the Token endpoint.", response.Exception);
            }

            var expiry = DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);

            return (response.AccessToken, response.IdentityToken, expiry);
        }

        public async Task<IEnumerable<Claim>> GetUserInfoAsync(string accessToken)
        {
            await LoadEndpointsAsync();

            var response = await _httpClient.GetUserInfoAsync(new UserInfoRequest()
            {
                Address = userInfoEndpoint,
                Token = accessToken
            });

            if (response.IsError)
            {
                throw new Exception("Problem accessing the UserInfo endpoint.", response.Exception);
            }

            return response.Claims;
        }

        public async Task<(string AccessToken, DateTime Expiry, string RefreshToken, string IdentityToken)> RenewTokensAsync(string refreshToken)
        {
            await LoadEndpointsAsync();

            var tokenResult = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest()
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scopes,
                RefreshToken = refreshToken
            });

            if (!tokenResult.IsError)
            {
                var expiry = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);

                return (tokenResult.AccessToken, expiry, tokenResult.RefreshToken, tokenResult.IdentityToken);
            }
            else
            {
                throw new Exception("Problem encountered while refreshing tokens.", tokenResult.Exception);
            }
        }

        public async Task RevokeAccessAsync(string accessToken, string refreshToken = null)
        {
            await LoadEndpointsAsync();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var revokeAccessTokenResponse = await _httpClient.RevokeTokenAsync(new TokenRevocationRequest()
                {
                    Address = revocationEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Token = accessToken,
                    TokenTypeHint = "access_token"
                });

                if (revokeAccessTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the access token.", revokeAccessTokenResponse.Exception);
                }
            }

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var revokeRefreshTokenResponse = await _httpClient.RevokeTokenAsync(new TokenRevocationRequest()
                {
                    Address = revocationEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Token = refreshToken,
                    TokenTypeHint = "refresh_token"
                });

                if (revokeRefreshTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the refresh token.", revokeRefreshTokenResponse.Exception);
                }
            }
        }
    }
}
