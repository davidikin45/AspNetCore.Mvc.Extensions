using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
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
            var newViewData = new ViewDataDictionary<TModel>(viewData) { Model = model };

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
                viewContext.HttpContext.RequestServices.GetRequiredService<ExpressionTextCache>());

            helper.Contextualize(newViewContext);

            return helper;
        }

    
    }
}
