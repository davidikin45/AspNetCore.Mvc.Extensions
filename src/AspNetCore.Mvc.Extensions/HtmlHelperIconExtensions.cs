using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperIconExtensions
    {
        public static HtmlString IconLink(this IHtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, String iconName, object htmlAttributes = null)
        {
            var linkMarkup = htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes).Render().Replace("%2F", "/");
            var iconMarkup = String.Format("<span class=\"{0}\" aria-hidden=\"true\"></span> ", iconName);
            return new HtmlString(linkMarkup.Insert(linkMarkup.IndexOf(@">") + 1, iconMarkup));
        }

        public static HtmlString IconLink(this IHtmlHelper htmlHelper, string linkText, string actionName, object routeValues, String iconName, object htmlAttributes = null)
        {
            var linkMarkup = htmlHelper.ActionLink(linkText, actionName, routeValues, htmlAttributes).Render().Replace("%2F", "/");
            var iconMarkup = String.Format("<span class=\"{0}\" aria-hidden=\"true\"></span> ", iconName);
            return new HtmlString(linkMarkup.Insert(linkMarkup.IndexOf(@">") + 1, iconMarkup));
        }
    }
}
