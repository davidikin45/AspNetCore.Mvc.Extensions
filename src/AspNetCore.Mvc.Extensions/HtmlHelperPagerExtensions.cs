using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperPagerExtensions
    {
        public static HtmlString BootstrapPager(this IHtmlHelper html, int currentPageIndex, Func<int, string> action, int totalItems, int pageSize = 10, int numberOfLinks = 5)
        {
            if (totalItems <= 0)
            {
                return HtmlString.Empty;
            }
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var lastPageNumber = (int)Math.Ceiling((double)currentPageIndex / numberOfLinks) * numberOfLinks;
            var firstPageNumber = lastPageNumber - (numberOfLinks - 1);
            var hasPreviousPage = currentPageIndex > 1;
            var hasNextPage = currentPageIndex < totalPages;
            if (lastPageNumber > totalPages)
            {
                lastPageNumber = totalPages;
            }

            var nav = new TagBuilder("nav");
            nav.AddCssClass("pagination-nav");

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination");
            ul.AddCssClass("justify-content-center");
            ul.InnerHtml.AppendHtml(AddLink(1, action, currentPageIndex == 1, "disabled", "<<", "First Page", false));
            ul.InnerHtml.AppendHtml(AddLink(currentPageIndex - 1, action, !hasPreviousPage, "disabled", "<", "Previous Page", false));
            for (int i = firstPageNumber; i <= lastPageNumber; i++)
            {
                ul.InnerHtml.AppendHtml(AddLink(i, action, i == currentPageIndex, "active", i.ToString(), i.ToString(), false));
            }
            ul.InnerHtml.AppendHtml(AddLink(currentPageIndex + 1, action, !hasNextPage, "disabled", ">", "Next Page", true));
            ul.InnerHtml.AppendHtml(AddLink(totalPages, action, currentPageIndex == totalPages, "disabled", ">>", "Last Page", false));

            nav.InnerHtml.AppendHtml(ul);


            return new HtmlString(nav.Render());
        }

        private static TagBuilder AddLink(int index, Func<int, string> action, bool condition, string classToAdd, string linkText, string tooltip, bool nextPage)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("page-item");
            li.MergeAttribute("title", tooltip);
            if (condition)
            {
                li.AddCssClass(classToAdd);
            }
            var a = new TagBuilder("a");
            a.AddCssClass("page-link");
            if (nextPage && !condition)
            {
                a.AddCssClass("pagination__next");
            }
            a.MergeAttribute("href", !condition ? action(index) : "javascript:");
            a.InnerHtml.Append(linkText);
            li.InnerHtml.AppendHtml(a);
            return li;
        }
    }
}
