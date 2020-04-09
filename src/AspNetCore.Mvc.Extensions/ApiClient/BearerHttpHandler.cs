using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    //Spa > Server > API
    //Spa > API > API

    //As long as OpenIdConnectOptions.SaveTokens or JwtBearerOptions.SaveTokens or IdentityServerAuthenticationOptions.SaveTokens = true
    public class BearerHttpHandler : DelegatingHandler
    {
        private readonly string accessToken;
        public BearerHttpHandler(IHttpContextAccessor httpContextAccessor)
        {
            accessToken =  httpContextAccessor.HttpContext.GetTokenAsync("access_token").GetAwaiter().GetResult();
        }

        public BearerHttpHandler(HttpMessageHandler innerHandler, IHttpContextAccessor httpContextAccessor)
            :base(innerHandler)
        {
            accessToken = httpContextAccessor.HttpContext.GetTokenAsync("access_token").GetAwaiter().GetResult();
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                //request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
