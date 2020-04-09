using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.Mvc.Extensions.UI
{
    public class UserSpecificationsMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public UserSpecificationsMvcOptionsSetup(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        public void Configure(MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new UserIncludeModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new UserFilterModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new UserOrderByModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new UserFieldsModelBinderProvider());
        }
    }
}
