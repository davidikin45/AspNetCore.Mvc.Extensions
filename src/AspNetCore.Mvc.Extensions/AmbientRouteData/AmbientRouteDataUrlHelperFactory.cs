using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.AmbientRouteData
{
    public class AmbientRouteDataUrlHelperFactory : IUrlHelperFactory
    {
        private readonly IUrlHelperFactory _helperFactory;
        private readonly AmbientRouteDataUrlHelperFactoryOptions _options;

        public AmbientRouteDataUrlHelperFactory(IUrlHelperFactory helperFactory, IOptions<AmbientRouteDataUrlHelperFactoryOptions> options)
        {
            _helperFactory = helperFactory;
            _options = options.Value;
        }

        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            var httpContext = context.HttpContext;

            var urlHelper = _helperFactory.GetUrlHelper(context);
            if (urlHelper.GetType().Name == "EndpointRoutingUrlHelper" || urlHelper.GetType().Name == "RoutingUrlHelper")
            {
                urlHelper = new AmbientRouteDataUrlHelper(urlHelper, _options);
                httpContext.Items[typeof(IUrlHelper)] = urlHelper;
            }

            return urlHelper;
        }
    }

    public class AmbientRouteDataUrlHelperFactoryOptions
    {
        public List<AmbientRouteData> AmbientRouteDataKeys = new List<AmbientRouteData>() {};
    }

    public class AmbientRouteData
    {
        public AmbientRouteData(string routeDataKey, bool roundTripUsingQueryString)
        {
            RouteDataKey = routeDataKey;
            RoundTripUsingQueryString = roundTripUsingQueryString;
        }

        public string RouteDataKey { get; set; }
        public bool RoundTripUsingQueryString { get; set; } = false;
    }
}
