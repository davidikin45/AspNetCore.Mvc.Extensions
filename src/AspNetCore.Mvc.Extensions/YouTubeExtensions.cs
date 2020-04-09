using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    public static class YouTubeExtensions
    {
        public static string YouTubeEmbedUrl(this string id)
        {
            return String.Format("https://www.youtube.com/embed/{0}", id);
        }

        public static string YouTubeMaxResThumbailUrl(this string id)
        {
            return String.Format("https://img.youtube.com/vi/{0}/maxresdefault.jpg", id);
        }
    }
}
