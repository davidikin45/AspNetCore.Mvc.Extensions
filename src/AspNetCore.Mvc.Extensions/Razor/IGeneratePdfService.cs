using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Razor
{
    public interface IGeneratePdfService
    {
        Task<IActionResult> DownloadViewPdfAsync(string fileDownloadName, string viewName, object model, object additionalViewData = null);
        Task<IActionResult> DisplayViewPdfAsync(string viewName, object model, object additionalViewData = null);
        Task<byte[]> GetViewPdfAsync(string viewName, object model, object additionalViewData = null);

        Task<IActionResult> DownloadViewHtmlPdfAsync(string fileDownloadName, string viewHtml, object model, object additionalViewData = null);
        Task<IActionResult> DisplayViewHtmlPdfAsync(string viewHtml, object model, object additionalViewData = null);
        Task<byte[]> GetViewHtmlPdfAsync(string viewHtml, object model, object additionalViewData = null);

        byte[] GetPdf(string html);

        void UpdateView(string path, string viewHtml);
        bool ExistsView(string path);
        void AddView(string path, string viewHtml);

    }
}
