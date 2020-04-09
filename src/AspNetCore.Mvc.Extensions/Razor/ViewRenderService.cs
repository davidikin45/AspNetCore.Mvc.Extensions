using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Razor
{
    //https://blog.elmah.io/generate-a-pdf-from-asp-net-core-for-free/?fbclid=IwAR0PONCCu6U83mYUGhwhxG6eADag-IRwsAejgrcGtzpvd1npBclfESoA8IQ
    //https://wkhtmltopdf.org/
    //Wkhtmltopdf.NetCore

    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ViewResultExecutor.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ViewExecutor.cs

    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/HtmlHelper.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/TemplateBuilder.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/TemplateRenderer.cs

    public class ViewRenderService : IViewRenderService
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UpdateableFileProvider _updateableFileProvider;

        protected MvcViewOptions ViewOptions { get; }

        public ViewRenderService(
            IModelMetadataProvider modelMetadataProvider,
            ICompositeViewEngine viewEngine,
            IHttpContextAccessor httpContextAccessor,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IOptions<MvcViewOptions> viewOptions,
            UpdateableFileProvider updateableFileProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _viewEngine = viewEngine;
            _httpContextAccessor = httpContextAccessor;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            ViewOptions = viewOptions.Value;
            _updateableFileProvider = updateableFileProvider;
        }

        public async Task<string> ViewAsync(string viewName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, viewName, null, true, false, false, additionalViewData);
                return writer.ToString();
            }
        }

        public async Task<string> ViewAsync(string viewName, object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, viewName, model, true, false, false, additionalViewData);
                return writer.ToString();
            }
        }

        public Task<string> ViewHtmlAsync(string viewHtml, object model, object additionalViewData = null)
        {
            _updateableFileProvider.UpdateContent(viewHtml);
            return ViewAsync("/Views/FakeView.cshtml", model, additionalViewData);
        }

        public Task<string> ViewHtmlAsync(string viewHtml, object additionalViewData = null)
        {
            _updateableFileProvider.UpdateContent(viewHtml);
            return ViewAsync("/Views/FakeView.cshtml", additionalViewData);
        }

        public async Task<string> PartialViewAsync(string templateName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, templateName, null, false, true, false, additionalViewData);
                return writer.ToString();
            }
        }

        public async Task<string> PartialViewAsync(string templateName, object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, templateName, model, false, true, false, additionalViewData);
                return writer.ToString();
            }
        }

        public async Task<string> DisplayForModelAsync(object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, null, model, false, false, true, additionalViewData);
                return writer.ToString();
            }
        }

        public async Task<string> DisplayForModelAsync(object model, string templateName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, templateName, model, false, false, true, additionalViewData);
                return writer.ToString();
            }
        }

        public async Task<string> EditorForModelAsync(object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, null, model, false, false, false, additionalViewData);
                return writer.ToString();
            }
        }

        public async Task<string> EditorForModelAsync(object model, string templateName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderToStringAsync(writer, templateName, model, false, false, false, additionalViewData);
                return writer.ToString();
            }
        }

        public string View(string viewName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, viewName, null, true, false, false, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }

        public string View(string viewName, object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, viewName, model, true, false, false, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }

        public string ViewHtml(string viewHtml, object model, object additionalViewData = null)
        {
            _updateableFileProvider.UpdateContent(viewHtml);
            return View("/Views/FakeView.cshtml", model, additionalViewData);
        }

        public string ViewHtml(string viewHtml, object additionalViewData = null)
        {
            _updateableFileProvider.UpdateContent(viewHtml);
            return View("/Views/FakeView.cshtml", additionalViewData);
        }

        public string PartialView(string templateName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, templateName, null, false, true, false, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }

        public string PartialView(string templateName, object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, templateName, model, false, true, false, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }

        public string DisplayForModel(object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {

                RenderToStringAsync(writer, null, model, false, false, true, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }

        public string DisplayForModel(object model, string templateName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, templateName, model, false, false, true, additionalViewData).GetAwaiter().GetResult();

                return writer.ToString();
            }
        }

        public string EditorForModel(object model, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, null, model, false, false, false, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }

        public string EditorForModel(object model, string templateName, object additionalViewData = null)
        {
            using (var writer = new StringWriter())
            {
                RenderToStringAsync(writer, templateName, model, false, false, false, additionalViewData).GetAwaiter().GetResult();
                return writer.ToString();
            }
        }


        private async Task RenderToStringAsync(TextWriter writer, string templateName, object model, bool mainPage, bool partial, bool readOnly, object additionalViewData)
        {
            var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var newViewData = new ViewDataDictionary<object>(_modelMetadataProvider, new ModelStateDictionary())
            {
                Model = model
            };

            var newTempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);



            if (mainPage || partial)
            {
                var result = _viewEngine.GetView(executingFilePath: null, viewPath: templateName, isMainPage: mainPage);
                if (!result.Success)
                {
                    result = _viewEngine.FindView(actionContext, templateName, isMainPage: mainPage);
                }

                if (!result.Success)
                {
                    throw new Exception($"A view with the name {templateName} could not be found");
                }

                var viewContext = new ViewContext(actionContext, result.View, newViewData, newTempData, writer, ViewOptions.HtmlHelperOptions)
                {
                    ExecutingFilePath = null
                };

                await viewContext.View.RenderAsync(viewContext); //Automatically writes out
            }
            else
            {
                var viewContext = new ViewContext()
                {
                    ExecutingFilePath = null,
                    View = null,
                    ViewData = newViewData,
                    TempData = newTempData,
                    Writer = writer,
                    FormContext = new FormContext(),
                    ClientValidationEnabled = ViewOptions.HtmlHelperOptions.ClientValidationEnabled,
                    Html5DateRenderingMode = ViewOptions.HtmlHelperOptions.Html5DateRenderingMode,
                    ValidationSummaryMessageElement = ViewOptions.HtmlHelperOptions.ValidationSummaryMessageElement,
                    ValidationMessageElement = ViewOptions.HtmlHelperOptions.ValidationMessageElement,
                    ActionDescriptor = actionContext.ActionDescriptor,
                    HttpContext = actionContext.HttpContext,
                    RouteData = actionContext.RouteData
                };

                var htmlHelper = MakeHtmlHelper(viewContext, newViewData);

                if (readOnly)
                {
                    //var htmlContent = htmlHelper.DisplayForModel(additionalViewData);
                    var htmlContent = htmlHelper.Display(null, templateName, null, additionalViewData);
                    htmlContent.WriteTo(writer, HtmlEncoder.Default);

                }
                else
                {
                    //var htmlContent = htmlHelper.EditorForModel(additionalViewData);
                    var htmlContent = htmlHelper.Editor(null, templateName, null, additionalViewData);
                    htmlContent.WriteTo(writer, HtmlEncoder.Default);
                }
            }
        }

        private IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary newViewData)
        {
            var newHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();

            if (newHelper is IViewContextAware contextable)
            {
                //var newViewContext = new ViewContext(viewContext, viewContext.View, newViewData, viewContext.Writer);
                var newViewContext = new ViewContext()
                {
                    ExecutingFilePath = null,
                    View = null,
                    ViewData = newViewData,
                    TempData = viewContext.TempData,
                    Writer = viewContext.Writer,
                    FormContext = viewContext.FormContext,
                    ClientValidationEnabled = viewContext.ClientValidationEnabled,
                    Html5DateRenderingMode = viewContext.Html5DateRenderingMode,
                    ValidationSummaryMessageElement = viewContext.ValidationSummaryMessageElement,
                    ValidationMessageElement = viewContext.ValidationMessageElement,
                    ActionDescriptor = viewContext.ActionDescriptor,
                    HttpContext = viewContext.HttpContext,
                    RouteData = viewContext.RouteData
                };

                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }


        public void AddView(string path, string viewHtml)
        {
            if (ExistsView(path))
            {
                throw new Exception($"View {path} already exists");
            }

            _updateableFileProvider.Views.Add($"/Views/{path}.cshtml", new ViewFileInfo(viewHtml));
        }

        public bool ExistsView(string path)
        {
            return _updateableFileProvider.Views.Any(x => x.Key == $"/Views/{path}.cshtml");
        }

        public void UpdateView(string path, string viewHtml)
        {
            if (ExistsView(path))
            {
                _updateableFileProvider.UpdateContent(viewHtml, $"/Views/{path}.cshtml");
            }
            else
            {
                throw new Exception($"View {path} does not exists");
            }
        }

    }
}

