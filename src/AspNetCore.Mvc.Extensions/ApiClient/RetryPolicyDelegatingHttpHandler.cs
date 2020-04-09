using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    public class RetryPolicyDelegatingHttpHandler : DelegatingHandler
    {
        private readonly int _maximumAmountOfRetries = 3;

        public RetryPolicyDelegatingHttpHandler(int maximumAmountOfRetries)
            : base()
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        public RetryPolicyDelegatingHttpHandler(HttpMessageHandler innerHandler,
          int maximumAmountOfRetries)
      : base(innerHandler)
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < _maximumAmountOfRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }
            return response;
        }
    }
}
