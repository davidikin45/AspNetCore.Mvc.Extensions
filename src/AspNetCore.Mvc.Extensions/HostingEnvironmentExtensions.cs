using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace AspNetCore.Mvc.Extensions
{
    public static class HostingEnvironmentExtensions
    {
        //public static string MapContentPath(this IHostingEnvironment hostingEnvironement, string path)
        //{
        //    var result = path ?? string.Empty;

        //    if (hostingEnvironement.IsContentPathMapped(path) == false)
        //    {
        //        var contentRoot = hostingEnvironement.ContentRootPath;
        //        if (result.StartsWith("~", StringComparison.Ordinal))
        //        {
        //            result = result.Substring(1);
        //        }
        //        if (result.StartsWith("/", StringComparison.Ordinal))
        //        {
        //            result = result.Substring(1);
        //        }
        //        result = Path.Combine(contentRoot, result.Replace('/', '\\'));

        //        if (!result.EndsWith(@"\") && !Path.GetFileName(result).Contains("."))
        //        {
        //            result = result + @"\";
        //        }
        //    }

        //    return result;
        //}

        //public static bool IsContentPathMapped(this IHostingEnvironment hostingEnvironement, string path)
        //{
        //    var result = path ?? string.Empty;

        //    return result.StartsWith(hostingEnvironement.ContentRootPath,
        //        StringComparison.Ordinal);
        //}

        //public static string MapWwwPath(this IHostingEnvironment hostingEnvironement, string path)
        //{
        //    var result = path ?? string.Empty;

        //    if (hostingEnvironement.IsWwwPathMapped(path) == false)
        //    {
        //        var wwwroot = hostingEnvironement.WebRootPath;
        //        if (result.StartsWith("~", StringComparison.Ordinal))
        //        {
        //            result = result.Substring(1);
        //        }
        //        if (result.StartsWith("/", StringComparison.Ordinal))
        //        {
        //            result = result.Substring(1);
        //        }
        //        result = Path.Combine(wwwroot, result.Replace('/', '\\'));

        //        if (!result.EndsWith(@"\") && !Path.GetFileName(result).Contains("."))
        //        {
        //            result = result + @"\";
        //        }
        //    }

        //    return result;
        //}

        //public static bool IsWwwPathMapped(this IHostingEnvironment hostingEnvironement, string path)
        //{
        //    var result = path ?? string.Empty;
        //    return result.StartsWith(hostingEnvironement.WebRootPath,
        //        StringComparison.Ordinal);
        //}

        //public static bool IsIntegration(this IHostingEnvironment hostingEnvironement)
        //{
        //    return hostingEnvironement.IsEnvironment("Integration");
        //}

        public static string MapContentPath(this IWebHostEnvironment hostingEnvironement, string path)
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

                if (!result.EndsWith(@"\") && !Path.GetFileName(result).Contains("."))
                {
                    result = result + @"\";
                }
            }

            return result;
        }

        public static bool IsContentPathMapped(this IWebHostEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;

            return result.StartsWith(hostingEnvironement.ContentRootPath,
                StringComparison.Ordinal);
        }

        public static string MapWwwPath(this IWebHostEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;

            if (hostingEnvironement.IsWwwPathMapped(path) == false)
            {
                var wwwroot = hostingEnvironement.WebRootPath;
                if (result.StartsWith("~", StringComparison.Ordinal))
                {
                    result = result.Substring(1);
                }
                if (result.StartsWith("/", StringComparison.Ordinal))
                {
                    result = result.Substring(1);
                }
                result = Path.Combine(wwwroot, result.Replace('/', '\\'));

                if (!result.EndsWith(@"\") && !Path.GetFileName(result).Contains("."))
                {
                    result = result + @"\";
                }
            }

            return result;
        }

        public static bool IsWwwPathMapped(this IWebHostEnvironment hostingEnvironement, string path)
        {
            var result = path ?? string.Empty;
            return result.StartsWith(hostingEnvironement.WebRootPath,
                StringComparison.Ordinal);
        }

        public static bool IsIntegration(this IHostEnvironment hostingEnvironement)
        {
            return hostingEnvironement.EnvironmentName == "Integration";
        }
    }
}
