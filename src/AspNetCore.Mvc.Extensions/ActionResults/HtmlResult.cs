using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{
    public class HtmlResult : ContentResult
    {
        public HtmlResult(string html)
        {
            Content = html;
            ContentType = "text/html";
            StatusCode = (int)HttpStatusCode.OK;
        }
    }
}
