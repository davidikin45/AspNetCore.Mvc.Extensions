using AspNetCore.Mvc.Extensions.APIs;
using HtmlTags;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperLayoutExtensions
    {
        public static HtmlString DisqusCommentCount(this IHtmlHelper helper, string path)
        {
            HtmlTag a = new HtmlTag("a");
            a.Attr("data-disqus-identifier", path);

            return new HtmlString(a.ToString());
        }

        public static HtmlString DisqusCommentCountScript(this IHtmlHelper helper, string disqusShortname)
        {
            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var disqus_shortname = '" + disqusShortname + "';");

            sb.AppendLine("(function () {");
            sb.AppendLine("var s = document.createElement('script');");
            sb.AppendLine("s.async = true;");
            sb.AppendLine("s.src = 'http://' + disqus_shortname + '.disqus.com/count.js';");
            sb.AppendLine("(document.getElementsByTagName('HEAD')[0] || document.getElementsByTagName('BODY')[0]).appendChild(s);");
            sb.AppendLine("}");
            sb.AppendLine("());");

            script.AppendHtml(sb.ToString());

            return new HtmlString(script.ToString());
        }

        public static HtmlString FacebookCommentsThread(this IHtmlHelper helper, string siteUrl, string path, string title)
        {
            var url = string.Format("{0}{1}", siteUrl, path);

            HtmlTag div = new HtmlTag("div");
            div.AddClass("fb-comments");
            div.Attr("data-href", url);
            div.Attr("data-numposts", "10");
            div.Attr("width", "100%");
            /*div.Attr("data-order-by", "social"); reverse_time /*/

            return new HtmlString(div.ToString());
        }

        public static HtmlString FacebookCommentCount(this IHtmlHelper helper, string siteUrl, string path)
        {
            var url = string.Format("{0}{1}", siteUrl, path);

            HtmlTag span = new HtmlTag("span");
            span.AddClass("fb-comments-count");
            span.Attr("data-href", url);

            return new HtmlString(span.ToString());
        }

        //ideally right after the opening<body> tag.
        public static HtmlString FacebookCommentsScript(this IHtmlHelper helper, string appid)
        {
            HtmlTag div = new HtmlTag("div");
            div.Id("fb-root");

            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" (function(d, s, id) { ");

            sb.AppendLine(" var js, fjs = d.getElementsByTagName(s)[0]; ");
            sb.AppendLine(" if (d.getElementById(id)) return; ");
            sb.AppendLine(" js = d.createElement(s); js.id = id; ");
            sb.AppendLine(" js.src = '//connect.facebook.net/en_US/sdk.js#xfbml=1&version=v2.9&appId=" + appid + "' ");
            sb.AppendLine(" fjs.parentNode.insertBefore(js, fjs); ");

            sb.AppendLine(" } ");
            sb.AppendLine(" (document, 'script', 'facebook-jssdk')); ");

            script.AppendHtml(sb.ToString());

            return new HtmlString(div.ToString() + script.ToString());
        }

        public static HtmlString GoogleAnalyticsScript(this IHtmlHelper helper, string trackingId)
        {
            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){ ");
            sb.AppendLine(" (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o), ");
            sb.AppendLine(" m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m) ");
            sb.AppendLine(" })(window,document,'script','https://www.google-analytics.com/analytics.js','ga'); ");
            sb.AppendLine(" ga('create', '" + trackingId + "', 'auto'); ");
            sb.AppendLine(" ga('send', 'pageview'); ");

            script.AppendHtml(sb.ToString());

            return new HtmlString(script.ToString());
        }

        public static HtmlString GoogleAdSenseScript(this IHtmlHelper helper, string id)
        {
            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");
            script.Attr("async", "");
            script.Attr("src", "//pagead2.googlesyndication.com/pagead/js/adsbygoogle.js");

            HtmlTag script2 = new HtmlTag("script");
            script2.Attr("type", "text/javascript");

            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine(" (adsbygoogle = window.adsbygoogle || []).push({ ");
            sb2.AppendLine(" google_ad_client: '" + id + "', ");
            sb2.AppendLine(" enable_page_level_ads: false ");
            sb2.AppendLine(" }); ");

            script2.AppendHtml(sb2.ToString());

            return new HtmlString(script.ToString() + script2.ToString());
        }

        public static HtmlString DisqusThread(this IHtmlHelper helper, string disqusShortname, string siteUrl, string path, string title)
        {
            HtmlTag div = new HtmlTag("div");
            div.Id("disqus_thread");

            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");

            var url = string.Format("{0}{1}", siteUrl, path);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var disqus_shortname = '" + disqusShortname + "';");

            sb.AppendLine("var disqus_config = function() {");
            sb.AppendLine("this.page.url = '" + url + "'");
            sb.AppendLine("this.page.identifier = '" + path + "'");
            sb.AppendLine("this.page.title = '" + title + "'");
            sb.AppendLine("};");

            sb.AppendLine("(function() {");
            sb.AppendLine("var dsq = document.createElement('script');");
            sb.AppendLine("dsq.type = 'text/javascript';");
            sb.AppendLine("dsq.async = false;");
            sb.AppendLine("dsq.src = 'http://' + disqus_shortname + '.disqus.com/embed.js';");
            sb.AppendLine("(document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);");
            sb.AppendLine("})();");

            script.AppendHtml(sb.ToString());

            HtmlTag noscript = new HtmlTag("noscript");
            noscript.AppendHtml("Please enable JavaScript to view the <a href=\"http://disqus.com/?ref_noscript\"> comments powered by Disqus.</a>");

            HtmlTag aDisqus = new HtmlTag("a");
            aDisqus.Attr("href", "http://disqus.com");
            aDisqus.AddClass("dsq-brlink");
            aDisqus.AppendHtml("blog comments powered by<span class=\"logo - disqus\">Disqus</span>");

            var finalHTML = div.ToString() + Environment.NewLine + script.ToString() + Environment.NewLine + noscript.ToString() + Environment.NewLine + aDisqus.ToString() + DisqusCommentCountScript(helper, disqusShortname).ToString();

            return new HtmlString(finalHTML);
        }

        public static HtmlString AddThisLinks(this IHtmlHelper helper, string siteUrl, string path, string title, string description, string imageUrl)
        {
            var url = string.Format("{0}{1}", siteUrl, path);

            HtmlTag div = new HtmlTag("div");
            div.Attr("class", "addthis_inline_share_toolbox_p3ki");
            div.Attr("data-url", url);
            div.Attr("data-title", title);
            div.Attr("data-description", description);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                div.Attr("data-media", imageUrl);
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString AddThisRelatedPosts(this IHtmlHelper helper)
        {

            HtmlTag div = new HtmlTag("div");
            div.Attr("class", "addthis_relatedposts_inline");

            return new HtmlString(div.ToString());
        }

        public static HtmlString ReturnToTop(this IHtmlHelper helper)
        {
            HtmlTag link = new HtmlTag("a");
            link.Attr("href", "javascript:");
            link.Id("return-to-top");
            link.AppendHtml(@"<i class=""fa fa-chevron-up""></i>");

            return new HtmlString(link.ToString());
        }

        public static HtmlString AddThisScript(this IHtmlHelper helper, string pubid)
        {
            HtmlTag script = new HtmlTag("script");
            script.Attr("src", "https://s7.addthis.com/js/300/addthis_widget.js#pubid=" + pubid);
            script.Attr("type", "text/javascript");

            return new HtmlString(script.ToString());
        }

        public static HtmlString PostScrollHeight(this IHtmlHelper helper)
        {
            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" var oldHeight = 0; ");
            sb.AppendLine(" window.setInterval(function() { ");
            sb.AppendLine(" if(oldHeight != document.body.scrollHeight) ");
            sb.AppendLine(" { ");
            sb.AppendLine(" oldHeight = document.body.scrollHeight; ");
            sb.AppendLine(" window.top.postMessage(document.body.scrollHeight, '*'); ");
            sb.AppendLine(" } ");
            sb.AppendLine(" }, 500);  ");

            script.AppendHtml(sb.ToString());

            return new HtmlString(script.ToString());
        }

        public static HtmlString DeferredIFrameLoad(this IHtmlHelper helper)
        {
            HtmlTag script = new HtmlTag("script");
            script.Attr("type", "text/javascript");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("function init() ");
            sb.AppendLine("{");
            sb.AppendLine("var vidDefer = document.getElementsByTagName('iframe');");
            sb.AppendLine("for (var i = 0; i < vidDefer.length; i++)");
            sb.AppendLine("{");
            sb.AppendLine("if (vidDefer[i].getAttribute('data-src'))");
            sb.AppendLine("{");
            sb.AppendLine("vidDefer[i].setAttribute('src', vidDefer[i].getAttribute('data-src'));");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("window.onload = init;");

            script.AppendHtml(sb.ToString());

            return new HtmlString(script.ToString());
        }

        public static HtmlString GoogleFontAsync(this IHtmlHelper helper, List<string> fonts, bool regular = true, bool bold = false, bool black = false, bool italic = false, bool boldItalic = false, Boolean hideTextWhileLoading = true, int timeout = 5000)
        {
            var html = Google.Font.GetFontScriptAsync(fonts, regular, bold, black, italic, boldItalic, hideTextWhileLoading, timeout);
            return new HtmlString(html);
        }

        public static HtmlString GetFontBodyCSSAsync(this IHtmlHelper helper, string font)
        {
            return new HtmlString(Google.Font.GetFontBodyCSSAsync(font));
        }

        public static HtmlString GetFontNavBarCSSAsync(this IHtmlHelper helper, string font, string styleCSS)
        {
            return new HtmlString(Google.Font.GetFontNavBarCSSAsync(font, styleCSS));
        }

        public static HtmlString GoogleFont(this IHtmlHelper helper, string font, string styleCSS, bool bodyFont, bool navBarFont, bool regular = true, bool bold = false, bool italic = false, bool boldItalic = false)
        {
            var html = Google.Font.GetFontLink(font, regular, bold, italic, boldItalic);
            if (bodyFont)
            {
                html += Environment.NewLine + Google.Font.GetFontBodyCSS(font);
            }

            if (navBarFont)
            {
                html += Environment.NewLine + Google.Font.GetFontNavBarCSS(font, styleCSS);
            }

            return new HtmlString(html);
        }

        public static HtmlString GoogleFontEffects(this IHtmlHelper helper, string[] effects)
        {
            var html = Google.Font.GetEffectsLink(effects);
            return new HtmlString(html);
        }
    }
}
