using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Encodings.Web;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string Controller(this IHtmlHelper htmlHelper)
        {
            var routeValues = htmlHelper.ViewContext.RouteData.Values;

            if (routeValues.ContainsKey("controller"))
                return (string)routeValues["controller"];

            return string.Empty;
        }

        public static string Action(this IHtmlHelper htmlHelper)
        {
            var routeValues = htmlHelper.ViewContext.RouteData.Values;

            if (routeValues.ContainsKey("action"))
                return (string)routeValues["action"];

            return string.Empty;
        }

        public static UrlHelper Url(this IHtmlHelper html)
        {
            return new UrlHelper(html.ViewContext);
        }

        public static IHtmlHelper<TModel> For<TModel>(this IHtmlHelper helper) where TModel : class, new()
        {
            return For<TModel>(helper.ViewContext, helper.ViewData);
        }

        public static IHtmlHelper<TModel> For<TModel>(this IHtmlHelper helper, TModel model)
        {
            return For<TModel>(helper.ViewContext, helper.ViewData, model);
        }

        public static IHtmlHelper<dynamic> For(this IHtmlHelper helper, dynamic model)
        {
            return For(helper.ViewContext, helper.ViewData, model);
        }

        public static IHtmlHelper<TModel> For<TModel>(ViewContext viewContext, ViewDataDictionary viewData) where TModel : class, new()
        {
            TModel model = new TModel();
            return For<TModel>(viewContext, viewData, model);
        }

        public static IHtmlHelper<TModel> For<TModel>(ViewContext viewContext, ViewDataDictionary viewData, TModel model)
        {
            var newViewData = new ViewDataDictionary<TModel>(viewData, model);

            ViewContext newViewContext = new ViewContext(
                viewContext,
                viewContext.View,
                newViewData,
                viewContext.Writer);

            var helper = new HtmlHelper<TModel>(
                viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlGenerator>(),
                viewContext.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>(),
                viewContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>(),
                viewContext.HttpContext.RequestServices.GetRequiredService<IViewBufferScope>(),
                viewContext.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>(),
                viewContext.HttpContext.RequestServices.GetRequiredService<UrlEncoder>(),
                viewContext.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>());

          //.NET Core 2.2 
         //   var helper = new HtmlHelper<TModel>(
         //(IHtmlGenerator)viewContext.HttpContext.RequestServices.GetService(typeof(IHtmlGenerator)),
         //(ICompositeViewEngine)viewContext.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)),
         //(IModelMetadataProvider)viewContext.HttpContext.RequestServices.GetService(typeof(IModelMetadataProvider)),
         //(IViewBufferScope)viewContext.HttpContext.RequestServices.GetService(typeof(IViewBufferScope)),
         //(HtmlEncoder)viewContext.HttpContext.RequestServices.GetService(typeof(HtmlEncoder)),
         //(UrlEncoder)viewContext.HttpContext.RequestServices.GetService(typeof(UrlEncoder)),
         //(ExpressionTextCache)viewContext.HttpContext.RequestServices.GetService(typeof(ExpressionTextCache)));


            helper.Contextualize(newViewContext);

            return helper;
        }


        public static IHtmlContent ActionLink<TController>(this IHtmlHelper html, Expression<Action<TController>> expression, string linkText, object htmlAttributes = null, Boolean passRouteValues = true) where TController : ControllerBase
        {
            var result = Helpers.ExpressionHelper.GetRouteValuesFromExpression<TController>(expression);
            result.RouteValues.Remove("Action");
            result.RouteValues.Remove("Controller");

            IDictionary<string, object> htmlAttributesDict = null;

            if (htmlAttributes != null)
            {
                htmlAttributesDict = (IDictionary<string, object>)new RouteValueDictionary(htmlAttributes);
            }
            else
            {
                htmlAttributesDict = new Dictionary<string, object>();
            }

            if (passRouteValues)
            {
                return html.ActionLink(linkText, result.Action, result.Controller, result.RouteValues, htmlAttributesDict);
            }
            else
            {
                return html.ActionLink(linkText, result.Action, result.Controller, new RouteValueDictionary(), htmlAttributesDict);
            }
        }
    }
}
