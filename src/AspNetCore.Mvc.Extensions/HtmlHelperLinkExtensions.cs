using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperLinkExtensions
    {
        public static string BuildUrlFromExpression<TController>(this IHtmlHelper helper, Expression<Action<TController>> action) where TController : ControllerBase
        {
            return LinkBuilder.BuildUrlFromExpression(helper.ViewContext.HttpContext, helper.ViewContext.HttpContext.RequestServices.GetRequiredService<LinkGenerator>(), action);
        }
    }
}
