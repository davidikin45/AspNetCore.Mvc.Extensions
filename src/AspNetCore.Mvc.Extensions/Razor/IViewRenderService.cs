using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Razor
{
    public interface IViewRenderService
    {
        string View(string viewName, object model, object additionalViewData = null);
        string View(string viewName, object additionalViewData = null);

        string ViewHtml(string viewHtml, object model, object additionalViewData = null);
        string ViewHtml(string viewHtml, object additionalViewData = null);

        string PartialView(string viewName, object model, object additionalViewData = null);
        string PartialView(string viewName, object additionalViewData = null);

        string DisplayForModel(object model, object additionalViewData = null);
        string DisplayForModel(object model, string templateName, object additionalViewData = null);
        string EditorForModel(object model, object additionalViewData = null);
        string EditorForModel(object model, string templateName, object additionalViewData = null);

        Task<string> ViewAsync(string viewName, object model, object additionalViewData = null);
        Task<string> ViewAsync(string viewName, object additionalViewData = null);

        Task<string> ViewHtmlAsync(string viewHtml, object model, object additionalViewData = null);
        Task<string> ViewHtmlAsync(string viewHtml, object additionalViewData = null);

        Task<string> PartialViewAsync(string viewName, object model, object additionalViewData = null);
        Task<string> PartialViewAsync(string viewName, object additionalViewData = null);

        Task<string> DisplayForModelAsync(object model, object additionalViewData = null);
        Task<string> DisplayForModelAsync(object model, string templateName, object additionalViewData = null);
        Task<string> EditorForModelAsync(object model, object additionalViewData = null);
        Task<string> EditorForModelAsync(object model, string templateName, object additionalViewData = null);

        void UpdateView(string path, string viewHtml);
        bool ExistsView(string path);
        void AddView(string path, string viewHtml);
    }
}
