using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.AmbientRouteData
{
    internal class AmbientRouteDataUrlHelper : UrlHelperBase
    {
        private readonly IUrlHelper _urlHelper;
        private readonly AmbientRouteDataUrlHelperFactoryOptions _options;

        public AmbientRouteDataUrlHelper(IUrlHelper urlHelper, AmbientRouteDataUrlHelperFactoryOptions options)
            :base(urlHelper.ActionContext)
        {
            _urlHelper = urlHelper;
            _options = options;
        }

        public new ActionContext ActionContext => _urlHelper.ActionContext;

        public override string Action(UrlActionContext actionContext)
        {
            var nonRoundTripUsingQueryStringValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            var values = GetValuesDictionary(actionContext.Values);

            foreach (var routeDataStringKey in _options.AmbientRouteDataKeys)
            {
                if (!values.ContainsKey(routeDataStringKey.RouteDataKey) &&
                  AmbientValues.TryGetValue(routeDataStringKey.RouteDataKey, out var value))
                {
                    if(!routeDataStringKey.RoundTripUsingQueryString)
                    {
                        nonRoundTripUsingQueryStringValues.Add(routeDataStringKey.RouteDataKey, value);
                    }

                    values[routeDataStringKey.RouteDataKey] = value;
                }
                else if (!values.ContainsKey(routeDataStringKey.RouteDataKey) && _urlHelper.ActionContext.HttpContext.Request.Query.TryGetValue(routeDataStringKey.RouteDataKey, out var queryStringValues))
                {
                    if (!routeDataStringKey.RoundTripUsingQueryString)
                    {
                        nonRoundTripUsingQueryStringValues.Add(routeDataStringKey.RouteDataKey, queryStringValues.First());
                    }

                    values[routeDataStringKey.RouteDataKey] = queryStringValues.First();
                }
            }
            
            actionContext.Values = values;

            var url = _urlHelper.Action(actionContext);

            if(url != null)
            {
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                    uri = new Uri($"http://www.domain.com{url}");

                var queryDictionary = QueryHelpers.ParseQuery(uri.Query);

                if (queryDictionary.Keys.Any(k => nonRoundTripUsingQueryStringValues.ContainsKey(k)))
                {
                    foreach (var key in queryDictionary.Keys.Where(k => nonRoundTripUsingQueryStringValues.ContainsKey(k)))
                    {
                        values.Remove(key);
                    }

                    actionContext.Values = values;
                    url = _urlHelper.Action(actionContext);
                }
            }

            return url;
        }

        private new RouteValueDictionary GetValuesDictionary(object values)
        {
            if (values is RouteValueDictionary routeValuesDictionary)
            {
                var routeValueDictionary = new RouteValueDictionary();
                foreach (var kvp in routeValuesDictionary)
                {
                    routeValueDictionary.Add(kvp.Key, kvp.Value);
                }

                return routeValueDictionary;
            }

            if (values is IDictionary<string, object> dictionaryValues)
            {

                var routeValueDictionary = new RouteValueDictionary();
                foreach (var kvp in dictionaryValues)
                {
                    routeValueDictionary.Add(kvp.Key, kvp.Value);
                }

                return routeValueDictionary;
            }

            return new RouteValueDictionary(values);
        }

        public override string Content(string contentPath)
        {
            return _urlHelper.Content(contentPath);
        }

        public override bool IsLocalUrl(string url)
        {
            return _urlHelper.IsLocalUrl(url);
        }

        public override string Link(string routeName, object values)
        {
            return _urlHelper.Link(routeName, values);
        }

        public override string RouteUrl(UrlRouteContext routeContext)
        {
            return _urlHelper.RouteUrl(routeContext);
        }
    }
}
