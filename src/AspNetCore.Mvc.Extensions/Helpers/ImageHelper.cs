using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    internal static class ImageHelper
    {
        public static Boolean IsImage(this FileInfo fileinfo)
        {
            return MimeMapping.GetMimeMapping(fileinfo.Name).StartsWith("image/");
        }

        public static Boolean IsVideo(this FileInfo fileinfo)
        {
            return IsVideo(fileinfo.Name);
        }

        public static Boolean IsVideo(this string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (ext == ".mp4")
                return true;

            return MimeMapping.GetMimeMapping(fileName).StartsWith("video/");
        }
    }
}
