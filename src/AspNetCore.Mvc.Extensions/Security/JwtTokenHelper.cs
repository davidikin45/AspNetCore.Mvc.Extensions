using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Security
{
    //https://www.carlrippon.com/asp-net-core-web-api-multi-tenant-jwts/
    public static class JwtTokenHelper
    {
        public static List<Claim> GetClaims(string userId, string userName, IEnumerable<string> roles, params string[] scopes)
        {
            var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId),
                            new Claim(ClaimTypes.Name, userName),
                            new Claim(JwtRegisteredClaimNames.Sub, userId),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.UniqueName, userName)
                        };

            // add scopes
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            //Add roles
            foreach (string role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            return claims;
        }

        //Assymetric
        public static JwtToken CreateJwtTokenSigningWithRsaSecurityKey(string userId, string userName, IEnumerable<string> roles, int? minuteExpiry, RsaSecurityKey key, string issuer, string audience, params string[] scopes)
        {
            var claims = GetClaims(userId, userName, roles, scopes);

            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            return CreateJwtToken(minuteExpiry, issuer, audience, claims, creds);
        }

        //Assymetric
        public static JwtToken CreateJwtTokenSigningWithCertificateSecurityKey(string userId, string userName, IEnumerable<string> roles, int? minuteExpiry, X509SecurityKey key, string issuer, string audience, params string[] scopes)
        {
            var claims = GetClaims(userId, userName, roles, scopes);
            var creds = new X509SigningCredentials(key.Certificate, SecurityAlgorithms.RsaSha256); // JwtSecurityTokenHandler will serialize X509SigningCredentials kid=key.KeyId(Thumbprint) and x5t=key.X5t(cert hash), it wont serialize x5t for SigningCredentials
            creds.Key.KeyId = key.KeyId;

            return CreateJwtToken(minuteExpiry, issuer, audience, claims, creds);
        }

        //Symmetric
        public static JwtToken CreateJwtTokenSigningWithKey(string userId, string userName, IEnumerable<string> roles, int? minuteExpiry, SymmetricSecurityKey key, string issuer, string audience, params string[] scopes)
        {
            var claims = GetClaims(userId, userName, roles, scopes);
     
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return CreateJwtToken(minuteExpiry, issuer, audience, claims, creds);
        }

        private static JwtToken CreateJwtToken(int? minuteExpiry, string issuer, string audience, List<Claim> claims, SigningCredentials creds)
        {
            var token = new JwtSecurityToken(
                        issuer,
                        audience,
                        claims,
                        expires: minuteExpiry.HasValue ? DateTime.UtcNow.AddMinutes(minuteExpiry.Value) : (DateTime?)null,
                        signingCredentials: creds);

            var results = new JwtToken
            {
                //https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/d895860414398b74727a7ef59c43626d2f51dd5f/src/System.IdentityModel.Tokens.Jwt/JwtSecurityTokenHandler.cs
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };

            return results;
        }

        public static JwtSecurityToken DecodeJwtToken(string jwt)
        {
            //https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/d895860414398b74727a7ef59c43626d2f51dd5f/src/System.IdentityModel.Tokens.Jwt/JwtSecurityTokenHandler.cs
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token;
        }
    }
}
