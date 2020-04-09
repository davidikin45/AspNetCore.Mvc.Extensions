using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    //Spa > API > API
    public class AuthorizationProxyHttpHandler : DelegatingHandler
    {
        private readonly string auth;

        public AuthorizationProxyHttpHandler(IHttpContextAccessor httpContextAccessor)
        {
            auth = httpContextAccessor.HttpContext?.Request
                      .Headers["Authorization"]
                      .FirstOrDefault();
        }

        public AuthorizationProxyHttpHandler(HttpMessageHandler innerHandler, IHttpContextAccessor httpContextAccessor)
            : base(innerHandler)
        {
            auth = httpContextAccessor.HttpContext?.Request
                      .Headers["Authorization"]
                      .FirstOrDefault();
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (auth != null)
                request.Headers.Add("Authorization", auth);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
