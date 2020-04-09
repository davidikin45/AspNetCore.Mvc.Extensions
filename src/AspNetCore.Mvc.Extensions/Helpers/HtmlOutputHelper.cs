using System;
using System.IO;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class HtmlOutputHelper
    {
        public static string RelativeToAbsoluteUrls(string html, string siteUrl)
        {
            StringWriter writer = new StringWriter();
            string baseUrl = siteUrl;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            foreach (var link in doc.DocumentNode.Descendants("link"))
            {
                link.Attributes["href"].Value = new Uri(new Uri(baseUrl), link.Attributes["href"].Value).AbsoluteUri;
            }

            foreach (var img in doc.DocumentNode.Descendants("img"))
            {
                img.Attributes["src"].Value = new Uri(new Uri(baseUrl), img.Attributes["src"].Value).AbsoluteUri;
            }

            foreach (var a in doc.DocumentNode.Descendants("a"))
            {
                a.Attributes["href"].Value = new Uri(new Uri(baseUrl), a.Attributes["href"].Value).AbsoluteUri;
            }

            doc.Save(writer);

            string newHtml = writer.ToString();

            return newHtml;
        }
    }
}
