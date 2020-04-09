using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{

    public class DownloadFileStreamResult : FileStreamResult
    {
        public DownloadFileStreamResult(Stream fileStream, string contentType, string fileDownloadName)
            : base(fileStream, contentType)
        {
            FileDownloadName = fileDownloadName;
        }
    }
}
