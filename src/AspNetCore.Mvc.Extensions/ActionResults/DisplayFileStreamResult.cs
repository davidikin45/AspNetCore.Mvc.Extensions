using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{

    public class DisplayFileStreamResult : FileStreamResult
    {
        public DisplayFileStreamResult(Stream fileStream, string contentType)
            : base(fileStream, contentType)
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
