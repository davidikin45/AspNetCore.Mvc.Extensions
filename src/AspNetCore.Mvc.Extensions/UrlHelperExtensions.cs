using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string Content(this IUrlHelper urlHelper, string path, bool toAbsolute, AppSettings appSettings)
        {
            var absoluteVirtual = urlHelper.Content(path);

            if (!toAbsolute)
            {
                return absoluteVirtual;
            }
            else
            {
                var siteUrl = appSettings.SiteUrl;
                if (string.IsNullOrWhiteSpace(siteUrl))
                {
                    siteUrl = urlHelper.ActionContext.HttpContext.Request.Host.ToUriComponent();
                }

                var absoluteUri = string.Concat(
                        urlHelper.ActionContext.HttpContext.Request.Scheme,
                        "://",
                       siteUrl,
                        urlHelper.ActionContext.HttpContext.Request.PathBase.ToUriComponent(),
                        absoluteVirtual);
                return absoluteUri;
            }
        }

        public static string Content(this IUrlHelper urlHelper, string folderId, string path, bool toAbsolute, AppSettings appSettings, IWebHostEnvironment hostingEnvironment)
        {
            var physicalPath = hostingEnvironment.MapWwwPath(appSettings.Folders[folderId]) + path;
            string absoluteVirtual = physicalPath.GetAbsoluteVirtualPath(hostingEnvironment);

            Uri url = new Uri(new Uri(appSettings.SiteUrl), absoluteVirtual);

            return toAbsolute ? url.AbsoluteUri : absoluteVirtual;
        }

        //public static string Content(this IUrlHelper urlHelper, string folderId, string path, bool toAbsolute, AppSettings appSettings, IHostingEnvironment hostingEnvironment)
        //{
        //    var physicalPath = hostingEnvironment.MapWwwPath(appSettings.Folders[folderId]) + path;
        //    string absoluteVirtual = physicalPath.GetAbsoluteVirtualPath(hostingEnvironment);

        //    Uri url = new Uri(new Uri(appSettings.SiteUrl), absoluteVirtual);

        //    return toAbsolute ? url.AbsoluteUri : absoluteVirtual;
        //}

        public static string AbsoluteRouteUrl(
            this IUrlHelper urlHelper,
            string routeName,
            object routeValues = null)
        {
            string scheme = urlHelper.ActionContext.HttpContext.Request.Scheme;
            return urlHelper.RouteUrl(routeName, routeValues, scheme);
        }

        public static string AbsoluteUrl<TController>(this IUrlHelper url, Expression<Action<TController>> expression, AppSettings appSettings, bool passRouteValues = true) where TController : ControllerBase
        {
            string absoluteUrl = "";
            var result = ExpressionHelper.GetRouteValuesFromExpression<TController>(expression);
            result.RouteValues.Remove("Action");
            result.RouteValues.Remove("Controller");

            if (passRouteValues)
            {
                absoluteUrl = AbsoluteUrl(url, result.Action, result.Controller, appSettings, result.RouteValues);
            }
            else
            {
                absoluteUrl = AbsoluteUrl(url, result.Action, result.Controller, appSettings);
            }

            return absoluteUrl;
        }

        public static string AbsoluteUrl(this IUrlHelper url, string actionName, string controllerName, AppSettings appSettings, object routeValues)
        {
            return AbsoluteUrl(url, actionName, controllerName, appSettings, new RouteValueDictionary(routeValues));
        }

        public static string AbsoluteUrl(this IUrlHelper url, string actionName, string controllerName, AppSettings appSettings, RouteValueDictionary routeValues = null)
        {
            var absoluteUrl = "";
            if (!string.IsNullOrEmpty(appSettings.SiteUrl))
            {
                absoluteUrl = string.Format("{0}{1}", appSettings.SiteUrl, url.Action(actionName, controllerName, routeValues));
            }
            else
            {
                //The trick here is that once you specify the protocol/scheme when calling any routing method, you get an absolute URL. I recommend this one when possible, but you also have the more generic way in the first example in case you need it.
                absoluteUrl = url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme).ToString();
            }
            return absoluteUrl;
        }

        public static string Action<TController>(this IUrlHelper url, Expression<Action<TController>> expression) where TController : Controller
        {
            var result = ExpressionHelper.GetRouteValuesFromExpression<TController>(expression);
            result.RouteValues.Remove("Action");
            result.RouteValues.Remove("Controller");

            return url.Action(result.Action, result.Controller, result.RouteValues);
        }

        public static string RouteUrl<TController>(this IUrlHelper url, Expression<Action<TController>> expression) where TController : Controller
        {
            var routeValues = ExpressionHelper.GetRouteValuesFromExpression<TController>(expression);
            return url.RouteUrl(routeValues);
        }
    }
}