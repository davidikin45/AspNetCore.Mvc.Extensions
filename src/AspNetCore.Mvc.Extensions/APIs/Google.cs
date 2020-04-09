using HtmlTags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspNetCore.Mvc.Extensions.APIs
{
    public class Google
    {
        public class Font
        {
            //sans-serif = Web Font
            //serif = Print
            //display = Headings
            //handwriting = HandWriting
            //coding = monospace

            private static ConcurrentDictionary<string, string> fontEffectURL = new ConcurrentDictionary<string, string>();

            // 'Aclonica:regular,bold,italic,bolditalic'
            public static string GetFontScriptAsync(List<string> fonts, bool regular = true, bool bold = false, bool black = false, bool italic = false, bool boldItalic = false, Boolean hideTextWhileLoading = true, int timeOut = 5000)
            {
                HtmlTag script = new HtmlTag("script");
                script.Attr("type", "text/javascript");

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(" WebFontConfig = {google:{families:[ ");
                foreach (string font in fonts)
                {
                    sb.AppendLine("'" + font + Google.Font.GetFontTypeString(regular, bold, black, italic, boldItalic) + "'");
                    if (fonts.Last() != font)
                    {
                        sb.AppendLine(",");
                    }
                }
                sb.AppendLine(" ]},timeout: " + timeOut.ToString() + "}; ");

                sb.AppendLine(" (function() { ");
                sb.AppendLine("  var wf = document.createElement('script'); ");
                sb.AppendLine("   wf.src = ('https:' == document.location.protocol ? 'https' : 'http') + '://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js'; ");
                sb.AppendLine("   wf.type = 'text/javascript'; ");
                sb.AppendLine("  wf.async = 'true'; ");
                sb.AppendLine(" var s = document.getElementsByTagName('script')[0]; ");
                sb.AppendLine("  s.parentNode.insertBefore(wf, s); ");
                sb.AppendLine(" })(); ");

                script.AppendHtml(sb.ToString());

                var finalString = script.ToString();

                if (hideTextWhileLoading)
                {
                    HtmlTag style = new HtmlTag("style");

                    style.AppendHtml(".wf-loading * { opacity: 0; }");

                    finalString = finalString + Environment.NewLine + style.ToString();
                }

                return finalString;
            }

            public static string GetFontLink(string font, bool regular = true, bool bold = false, bool black = false, bool italic = false, bool boldItalic = false)
            {
                string[] fonts = { font };

                HtmlTag link = new HtmlTag("link");
                link.Attr("href", GetFontURL(fonts, regular, bold, black, italic, boldItalic));
                link.Attr("rel", "stylesheet");

                return link.ToString();
            }

            public static string GetFontBodyCSSAsync(string font)
            {
                HtmlTag style = new HtmlTag("style");

                style.AppendHtml(".wf-active body { font-family: '" + font + "', sans-serif !important; }");

                return style.ToString();
            }

            public static string GetFontNavBarCSSAsync(string font, string styleCSS)
            {
                HtmlTag style = new HtmlTag("style");

                style.AppendHtml(" .wf-active .nav-item { font-family: '" + font + "', sans-serif !important; } ");
                style.AppendHtml(" .wf-active .nav-link { " + styleCSS + " } ");

                return style.ToString();
            }

            public static string GetFontBodyCSS(string font)
            {
                HtmlTag style = new HtmlTag("style");

                style.AppendHtml("body { font-family: '" + font + "', sans-serif !important; }");

                return style.ToString();
            }

            public static string GetFontNavBarCSS(string font, string styleCSS)
            {
                HtmlTag style = new HtmlTag("style");

                style.AppendHtml(".nav-item { font-family: '" + font + "', sans-serif !important; } ");
                style.AppendHtml(".nav-link { " + styleCSS + " } ");

                return style.ToString();
            }

            public static string GetFontURL(string[] font, bool regular = true, bool bold = false, bool black = false, bool italic = false, bool boldItalic = false)
            {
                List<string> list = ((IEnumerable<string>)font).ToList<string>();
                List<string> newList = new List<string>();

                list.ForEach((Action<string>)(x => newList.Add(x + Google.Font.GetFontTypeString(regular, bold, black, italic, boldItalic))));

                string key = string.Join(":", (IEnumerable<string>)newList);
                if (Google.Font.fontEffectURL.ContainsKey(key))
                    return Google.Font.fontEffectURL[key];
                string Left = "http://fonts.googleapis.com/css?family=" + string.Join("|", (IEnumerable<string>)newList).Replace(" ", "+");
                if (Left != "")
                    Google.Font.fontEffectURL.AddOrUpdate(key, Left, (oldkey, oldvalue) => Left);
                return Left;
            }

            public static string GetEffectsLink(string[] effects)
            {
                HtmlTag link = new HtmlTag("link");
                link.Attr("href", GetEffectsURL(effects));
                link.Attr("rel", "stylesheet");

                return link.ToString();
            }

            public static string GetEffectsURL(string[] effects)
            {
                string key = string.Join(":", effects);
                if (Google.Font.fontEffectURL.ContainsKey(key))
                    return Google.Font.fontEffectURL[key];
                string Left = "http://fonts.googleapis.com/css?effect=" + string.Join("|", effects).Replace(" ", "+");
                if (Left != "")
                    Google.Font.fontEffectURL.AddOrUpdate(key, Left, (oldkey, oldvalue) => Left);
                return Left;
            }

            public static string GetFontAndEffectsURL(string[] font, string[] effects, bool regular = true, bool bold = false, bool black = false, bool italic = false, bool boldItalic = false)
            {
                List<string> list = ((IEnumerable<string>)font).ToList<string>();
                string str;
                list.ForEach((Action<string>)(x => str = x + Google.Font.GetFontTypeString(regular, bold, black, italic, boldItalic)));
                string key = string.Join(":", (IEnumerable<string>)list);
                if (effects.Length > 0)
                    key = key + ":" + string.Join(":", effects);
                if (Google.Font.fontEffectURL.ContainsKey(key))
                    return Google.Font.fontEffectURL[key];
                string Left = "http://fonts.googleapis.com/css?family=" + string.Join("|", (IEnumerable<string>)list).Replace(" ", "+") + "&effect=" + string.Join("|", effects).Replace(" ", "+");
                if (Left != "")
                    Google.Font.fontEffectURL.AddOrUpdate(key, Left, (oldkey, oldvalue) => Left);
                return Left;
            }

            private static string GetFontTypeString(bool regular, bool bold, bool black, bool italic, bool boldItalic)
            {
                List<string> stringList = new List<string>();
                if (regular)
                    stringList.Add("regular");
                if (bold)
                    stringList.Add("bold");
                if (black)
                    stringList.Add("900");
                if (italic)
                    stringList.Add("italic");
                if (boldItalic)
                    stringList.Add("bolditalic");
                return ":" + string.Join(",", (IEnumerable<string>)stringList);
            }
        }
    }
}
