using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetCore.Mvc.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidUrl(this string text)
        {
            var rx = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            return rx.IsMatch(text);
        }

        public static Boolean IsValidIp(this string ip)
        {
            if (!Regex.IsMatch(ip, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
                return false; var ips = ip.Split('.');
            if (ips.Length == 4 || ips.Length == 6)
            {
                return Int32.Parse(ips[0]) < 256 && System.Int32.Parse(ips[1]) < 256
                       & Int32.Parse(ips[2]) < 256 & System.Int32.Parse(ips[3]) < 256;
            }
            return false;
        }

        public static string CamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (!char.IsUpper(s[0]))
                return s;
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;
                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }
            return new string(chars);
        }

        public static string PascalCase(this string s)
        {
            // If there are 0 or 1 characters, just return the string.
            if (s == null) return s;
            if (s.Length < 2) return s.ToUpper();

            // Split the string into words.
            string[] words = s.Split(
                new char[] { ' ', '-', '_' },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }


        public static string GetStringWithSpacesAndFirstLetterUpper(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var value = Regex.Replace(
               input,
               "(?<!^)" +
               "(" +
               "  [A-Z][a-z] |" +
               "  (?<=[a-z])[A-Z] |" +
               "  (?<![A-Z])[A-Z]$" +
               ")",
               " $1",
               RegexOptions.IgnorePatternWhitespace);

                value = value.Replace("-", " ").Replace("_", " ");

                var chars = value.ToCharArray();
                chars[0] = char.ToUpper(chars[0], CultureInfo.InvariantCulture);

                return new string(chars); ;
            }
            else
            {
                return input;
            }
        }

        public static string ReplaceFromDictionary(this string s, Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                s = s.Replace(kvp.Key, kvp.Value);
            }
            return s;
        }

        public static string ToQueryString(this Dictionary<string, string> dict)
        {
            var builder = new StringBuilder();
            var count = 0;
            foreach (KeyValuePair<string, string> kvp in dict)
            {

                builder.Append(System.Net.WebUtility.UrlEncode(kvp.Key) + "=" + System.Net.WebUtility.UrlEncode(kvp.Value));
                if (count != dict.Keys.Count - 1)
                {
                    builder.Append("&");
                }
                count++;

            }
            return builder.ToString();
        }

        public static string Truncate(this string source, int length)
        {
            if (source != null)
            {
                if (source.Length > length)
                {
                    source = source.Substring(0, length) + "...";
                }
            }
            return source;
        }

        public static int CountStringOccurrences(this string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

    }
}
