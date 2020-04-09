using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    public static class PolicyHolder
    {
        public static AsyncPolicyWrap<HttpResponseMessage> GetRequestPolicy(IMemoryCache memoryCache = null, int cacheSeconds = 0, int additionalRetries = 0, int requestTimeoutSeconds = 100)
        {
            AsyncCachePolicy cache = null;
            if (memoryCache != null)
            {
                var memoryCacheProvider = new MemoryCacheProvider(memoryCache);
                cache = Policy.CacheAsync(memoryCacheProvider, TimeSpan.FromSeconds(cacheSeconds));
            }

            int[] httpStatusCodesWorthRetrying = {
               StatusCodes.Status408RequestTimeout,
               StatusCodes.Status429TooManyRequests,
               //StatusCodes.Status500InternalServerError,
               StatusCodes.Status502BadGateway,
               StatusCodes.Status503ServiceUnavailable,
               StatusCodes.Status504GatewayTimeout
            };

            var waitAndRetryPolicy = Policy
             .Handle<HttpRequestException>() //HttpClient Timeout or CancellationToken
             .Or<TimeoutRejectedException>()
             .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains((int)r.StatusCode))
             .WaitAndRetryAsync(additionalRetries,
              retryAttempt => TimeSpan.FromSeconds(1));

            //https://github.com/App-vNext/Polly/wiki/Timeout
            var requestTimeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(requestTimeoutSeconds));

            //https://github.com/App-vNext/Polly/wiki/PolicyWrap
            AsyncPolicyWrap<HttpResponseMessage> policyWrap = null;
            if (cache != null)
            {
                policyWrap = cache.WrapAsync(waitAndRetryPolicy).WrapAsync(requestTimeout);
            }
            else
            {
                policyWrap = waitAndRetryPolicy.WrapAsync(requestTimeout);
            }

            return policyWrap;
        }
    }
}
