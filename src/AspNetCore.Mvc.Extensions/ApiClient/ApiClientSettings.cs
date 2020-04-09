using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    public class ApiClientSettings
    {
        public string BaseUrl { get; set; } = "";
        public int MaxTimeoutSeconds { get; set; } = 100;

        public Dictionary<string, ApiClientMethodSettings> MethodSettings { get; set; } = new Dictionary<string, ApiClientMethodSettings>() {

        };
    }

    public class ApiClientMethodSettings
    {
        public ApiClientMethodSettings()
        {

        }

        public ApiClientMethodSettings(string url)
        {
            Url = url;
        }

        public string Url { get; set; }

        public int CacheSeconds { get; set; } = 0;
        public int RetryCount { get; set; } = 0;
        public int TimeoutSeconds { get; set; } = 100;
    }
}
