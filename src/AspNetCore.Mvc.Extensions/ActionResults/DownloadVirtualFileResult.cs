using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.ActionResults
{
    public class DownloadVirtualFileResult : VirtualFileResult
    {
        public DownloadVirtualFileResult(string virtualPath, string contentType, string fileDownloadName)
            : base(virtualPath, contentType)
        {
            FileDownloadName = fileDownloadName;
        }
    }
}

