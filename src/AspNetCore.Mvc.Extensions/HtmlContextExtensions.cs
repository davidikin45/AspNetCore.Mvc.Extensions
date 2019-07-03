using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlContextExtensions
    {
        public static string Render(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
