using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Middleware
{
    public class ResponseCachingCustomMiddleware : ResponseCachingMiddleware
    {
        public static IOptions<ResponseCachingOptions> Options;
        public static ILoggerFactory LoggerFactory;
        public static ObjectPoolProvider PoolProvider;

        public static ResponseCachingCustomMiddleware Instance;

        public ResponseCachingCustomMiddleware(
            RequestDelegate next,
            IOptions<ResponseCachingOptions> options,
            ILoggerFactory loggerFactory,
            ObjectPoolProvider poolProvider)
            : base(
                next,
                options,
                loggerFactory,
                poolProvider)
        {
            Options = options;
            LoggerFactory = loggerFactory;
            PoolProvider = poolProvider;

            loggerFactory.CreateLogger<ResponseCachingMiddleware>().LogInformation("Response Caching Middleware Initialised");
            Instance = this;
        }

        public static void ClearResponseCache()
        {
            if( Instance != null)
            {
                ResponseCachingOptions options = (ResponseCachingOptions)(typeof(ResponseCachingMiddleware).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Instance));

                long sizeLimit = options.SizeLimit;

                var newResponseCachingMiddleware = new ResponseCachingMiddleware((context) => Task.CompletedTask, Options, LoggerFactory, PoolProvider);

                var fieldInfo = typeof(ResponseCachingMiddleware).GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic);

                var newCache = fieldInfo.GetValue(newResponseCachingMiddleware);

                fieldInfo.SetValue(Instance, newCache);
            }           
        }
    }

    //.NET Core 2.2 
    //public class ResponseCachingCustomMiddleware : ResponseCachingMiddleware
    //{
    //    public static ResponseCachingCustomMiddleware Instance;

    //    public ResponseCachingCustomMiddleware(
    //        RequestDelegate next,
    //        IOptions<ResponseCachingOptions> options,
    //        ILoggerFactory loggerFactory,
    //        IResponseCachingPolicyProvider policyProvider,
    //        IResponseCachingKeyProvider keyProvider)
    //        : base(
    //            next,
    //            options,
    //            loggerFactory,
    //            policyProvider,
    //            keyProvider)
    //    {
    //        loggerFactory.CreateLogger<ResponseCachingMiddleware>().LogInformation("Response Caching Middleware Initialised");
    //        Instance = this;
    //    }

    //    public static void ClearResponseCache()
    //    {
    //        if (Instance != null)
    //        {
    //            ResponseCachingOptions options = (ResponseCachingOptions)(typeof(ResponseCachingMiddleware).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Instance));

    //            long sizeLimit = options.SizeLimit;

    //            var newCache = new MemoryResponseCache(new MemoryCache(new MemoryCacheOptions
    //            {
    //                SizeLimit = sizeLimit
    //            }));

    //            typeof(ResponseCachingMiddleware)
    //           .GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic)
    //           .SetValue(Instance, newCache);
    //        }
    //    }
    //}

}
