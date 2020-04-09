using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{
    public class DownloadFileContentResult : FileContentResult
    {
        public DownloadFileContentResult(byte[] fileContents, string contentType, string fileDownloadName)
            : base(fileContents, contentType)
        {
            FileDownloadName = fileDownloadName;
        }
        
    }
}

