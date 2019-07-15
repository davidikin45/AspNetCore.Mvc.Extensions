using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Middleware
{
    public class IPFilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _whiteListIPList;
        public IPFilterMiddleware(RequestDelegate next, string[] whiteList)
        {
            _next = next;
            _whiteListIPList = whiteList;
        }
        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
            var isInwhiteListIPList = _whiteListIPList
                .Where(a => IPAddress.Parse(a)
                .Equals(ipAddress))
                .Any();

            if (!isInwhiteListIPList)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
