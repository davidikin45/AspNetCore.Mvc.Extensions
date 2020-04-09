using AspNetCore.Mvc.Extensions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions
{
    public static class LinkBuilder
    {
        public static string BuildUrlFromExpression<TController>(HttpContext httpContext, LinkGenerator linkGenerator, Expression<Action<TController>> action) where TController : ControllerBase
        {
            var result = ExpressionHelper.GetRouteValuesFromExpression(action);
            return linkGenerator.GetPathByAction(httpContext, result.Action, result.Controller, result.RouteValues);
        }

    }
}
