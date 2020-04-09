using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly.Wrap;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    public abstract class ApiClientService
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly ApiClientSettings _clientSettings;
        private readonly JsonSerializerSettings _serializerSettings;

        protected ApiClientService(string baseUrl, int maxTimeoutSeconds = 100)
          : this(new HttpClient() { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(maxTimeoutSeconds) }, new ApiClientSettings() { BaseUrl = baseUrl, MaxTimeoutSeconds = maxTimeoutSeconds }, new MemoryCache(new MemoryCacheOptions()), null)
        {

        }

        protected ApiClientService(ApiClientSettings apiClientSettings)
           : this(new HttpClient() { BaseAddress = new Uri(apiClientSettings.BaseUrl), Timeout = TimeSpan.FromSeconds(apiClientSettings.MaxTimeoutSeconds) }, apiClientSettings, new MemoryCache(new MemoryCacheOptions()), null)
        {

        }

        public ApiClientService(HttpClient client, ApiClientSettings apiClientSettings, IMemoryCache memoryCache = null, ILogger logger = null)
        {
            _client = client;
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger = logger;
            _clientSettings = apiClientSettings;

            _serializerSettings = new JsonSerializerSettings();
            _serializerSettings.Formatting = Formatting.Indented;
            _serializerSettings.DateParseHandling = DateParseHandling.DateTime;
            _serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            _serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            _memoryCache = memoryCache;
        }

        #region Policy
        private IMemoryCache _memoryCache;

        public void ClearCache()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        private AsyncPolicyWrap<HttpResponseMessage> GetRequestPolicy(int cacheSeconds = 0, int additionalRetries = 0, int requestTimeoutSeconds = 100)
        {
            return PolicyHolder.GetRequestPolicy(_memoryCache, cacheSeconds, additionalRetries, requestTimeoutSeconds);
        }

        #endregion
    }
}
