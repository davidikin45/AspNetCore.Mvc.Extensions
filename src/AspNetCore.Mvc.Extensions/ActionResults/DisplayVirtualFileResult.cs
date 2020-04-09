using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{
    public class DisplayVirtualFileResult : VirtualFileResult
    {
        public DisplayVirtualFileResult(string virtualPath, string contentType)
            : base(virtualPath, contentType)
        {
    
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var contentDispositionHeader = new ContentDisposition
            {
                FileName = FileDownloadName,
                Inline = true,
            };
            context.HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
            FileDownloadName = null;
            context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            return base.ExecuteResultAsync(context);
        }
    }
}

