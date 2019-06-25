using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace AspNetCore.Mvc.Extensions.Extensions
{
    public static class HostingEnvironmentExtensions
    {
        public static string MapContentPath(this IHostingEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;
            if (hostingEnvironement.IsContentPathMapped(path) == false)
            {
                var contentRoot = hostingEnvironement.ContentRootPath;
                if (result.StartsWith("~", StringComparison.Ordinal))
                {
                    result = result.Substring(1);
                }
                if (result.StartsWith("/", StringComparison.Ordinal))
                {
                    result = result.Substring(1);
                }
                result = Path.Combine(contentRoot, result.Replace('/', '\\'));
            }

            return result;
        }

        public static bool IsContentPathMapped(this IHostingEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;
            return result.StartsWith(hostingEnvironement.ContentRootPath,
                StringComparison.Ordinal);
        }

        public static string MapWwwPath(this IHostingEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;
            if (hostingEnvironement.IsWwwPathMapped(path) == false)
            {
                var wwwroot = hostingEnvironement.WebRootPath + @"\";
                if (result.StartsWith("~", StringComparison.Ordinal))
                {
                    result = result.Substring(1);
                }
                if (result.StartsWith("/", StringComparison.Ordinal))
                {
                    result = result.Substring(1);
                }
                result = Path.Combine(wwwroot, result.Replace('/', '\\'));
            }

            return result;
        }

        public static bool IsWwwPathMapped(this IHostingEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;
            return result.StartsWith(hostingEnvironement.WebRootPath + @"\",
                StringComparison.Ordinal);
        }

        public static bool IsIntegration(this IHostingEnvironment hostingEnvironement)
        {
            return hostingEnvironement.IsEnvironment("Integration");
        }
    }
}
