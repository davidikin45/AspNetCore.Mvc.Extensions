using AspNetCore.Mvc.Extensions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AspNetCore.Mvc.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder, params string[] whiteList)
        {
            return builder.UseMiddleware<IPFilterMiddleware>(whiteList);
        }
    }
}
