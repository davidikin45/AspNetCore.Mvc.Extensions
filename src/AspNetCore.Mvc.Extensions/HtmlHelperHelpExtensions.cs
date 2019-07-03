using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperHelpExtensions
    {
        public static IHtmlContent HelpText(this IHtmlHelper helper, string helpText)
        {
            var small = new TagBuilder("small");
            small.AddCssClass("form-text");
            small.AddCssClass("text-muted");
            small.InnerHtml.SetContent(helpText);

            return small;
        }
    }
}
