using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperMenuExtensions
    {
        public static HtmlString MenuLink(this IHtmlHelper helper, string area, string controllerName, string actionName, string linkText, string classNames, string iconClass = "", string content = "")
        {
            var routeData = helper.ViewContext.RouteData.Values;
            var currentController = routeData["controller"];
            var currentAction = routeData["action"];

            var urlHelperFactory = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(helper.ViewContext);
            var linkGenerator = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();

            if (String.Equals(actionName, currentAction as string,
                      StringComparison.OrdinalIgnoreCase)
                &&
               String.Equals(controllerName, currentController as string,
                       StringComparison.OrdinalIgnoreCase))

            {
                classNames = classNames + " active";

                HtmlString activeLink = null;

                if (!string.IsNullOrEmpty(iconClass))
                {
                    activeLink = new HtmlString(string.Format(@"<a href=""{0}"" class=""{1}"" title=""{2}""><i class=""fa {3}""></i></a>", urlHelper.Action(actionName, controllerName, new { area }), classNames, linkText, iconClass));
                }
                else if (!string.IsNullOrEmpty(content))
                {
                    activeLink = new HtmlString(string.Format(@"<a href=""{0}"" class=""{1}"" title=""{2}"">{3}{2}</a>", urlHelper.Action(actionName, controllerName, new { area }), classNames, linkText, content));
                }
                else
                {
                    activeLink = new HtmlString(helper.ActionLink(linkText, actionName, controllerName, new { area },
                    new { @class = classNames, title = linkText }
                    ).Render());
                }

                return activeLink;
            }

            HtmlString link = null;


            if (!string.IsNullOrEmpty(iconClass))
            {
                link = new HtmlString(string.Format(@"<a href=""{0}"" class=""{1}"" title=""{2}""><i class=""fa {3}""></i></a>", urlHelper.Action(actionName, controllerName, new { area }), classNames, linkText, iconClass));
            }
            else if (!string.IsNullOrEmpty(content))
            {
                link = new HtmlString(string.Format(@"<a href=""{0}"" class=""{1}"" title=""{2}"">{3}{2}</a>", urlHelper.Action(actionName, controllerName, new { area }), classNames, linkText, content));
            }
            else
            {
                link = new HtmlString(helper.ActionLink(linkText, actionName, controllerName, new { area },
                new { @class = classNames, title = linkText }
                ).Render());
            }

            return link;
        }

        public static IHtmlContent MenuLink(this IHtmlHelper helper, string area, string controllerName, string actionName, string linkText, string classNames)
        {
            if (controllerName != "")
            {
                var routeData = helper.ViewContext.RouteData.Values;
                var currentController = routeData["controller"];
                var currentAction = routeData["action"];

                if (String.Equals(actionName, currentAction as string,
                          StringComparison.OrdinalIgnoreCase)
                    &&
                   String.Equals(controllerName, currentController as string,
                           StringComparison.OrdinalIgnoreCase))

                {
                    classNames = classNames + " active";
                    var activeLink = helper.ActionLink(linkText, actionName, controllerName, new { area },
                        new { @class = classNames, title = linkText, itemprop = "url" }
                        );
                    return activeLink;

                }

                var link = helper.ActionLink(linkText, actionName, controllerName, new { area },
                    new { @class = classNames, title = linkText, itemprop = "url" }
                    );
                return link;
            }
            else
            {
                return new HtmlString(string.Format("<a href='{0}' itemprop='url' class='{1}'>{2}</a>", actionName, classNames, linkText));
            }
        }
    }
}
