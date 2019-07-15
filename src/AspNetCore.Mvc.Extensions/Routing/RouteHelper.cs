using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Routing
{
    public static class RouteHelper
    {
        public static RouteInfo GetAllRoutes(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            var actions = actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(a => new ActionRouteInfo
            {
                Action = a.RouteValues["action"],
                Controller = a.RouteValues["controller"],
                Area = a.RouteValues.ContainsKey("area") ? a.RouteValues["area"] : null,
                Order = a?.AttributeRouteInfo?.Order,
                Name = a?.AttributeRouteInfo?.Name,
                Template = a?.AttributeRouteInfo?.Template,
                Path = a.GetRoutePath(),
                HttpMethods = a?.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods,
                Authorized = a.GetCustomAttributes<AuthorizeAttribute>().Any()
            });

            return new RouteInfo
            {
                Actions = actions
            };
        }

        private static string GetRoutePath(this ActionDescriptor actionDescriptor)
        {
            string path = null;
            if (actionDescriptor is PageActionDescriptor)
            {
                var e = actionDescriptor as PageActionDescriptor;
                path = e.ViewEnginePath;
            }

            if (actionDescriptor.AttributeRouteInfo != null)
            {
                var e = actionDescriptor;
                path = $"/{e.AttributeRouteInfo.Template}";
            }

            return path;
        }

        private static IEnumerable<T> GetCustomAttributes<T>(this ActionDescriptor actionDescriptor) where T : Attribute
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                return controllerActionDescriptor.MethodInfo.ReflectedType.GetCustomAttributes(typeof(T), true).Select(a => (T)a);
            }

            return Enumerable.Empty<T>();
        }
    }

    public class ActionRouteInfo
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public int? Order { get; set; }
        public string Name { get; set; }
        public string Template { get; set; }
        public string Path { get; set; }
        public IEnumerable<string> HttpMethods { get; set; }
        public bool Authorized { get; set; }
    }

    public class RouteInfo
    {
        public IEnumerable<ActionRouteInfo> Actions { get; set; }
    }
}
