using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HttpClientREST
{
    //https://gunnarpeipman.com/serialize-url-encoded-form-data/
    //https://geeklearning.io/serialize-an-object-to-an-url-encoded-string-in-csharp/
    //https://www.hanselman.com/blog/ASPNETWireFormatForModelBindingToArraysListsCollectionsDictionaries.aspx
    public static class QueryStringHelper
    {
        public static async Task<string> ToQueryStringUrlEncodedAsync(object obj)
        {
            var keyValueContent = ToKeyValue(obj);
            var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent ??  new Dictionary<string, string>());
            var urlEncodedString = await formUrlEncodedContent.ReadAsStringAsync();
            return urlEncodedString;
        }

        public static Dictionary<string, StringValues> ToDictionary(object obj)
        {
            return QueryHelpers.ParseQuery(ToQueryString(obj));
        }

        public static string ToQueryString(object obj)
        {
            var keyValueContent = ToKeyValue(obj);
            return String.Join("&", keyValueContent.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        public static FormUrlEncodedContent ToFormData(object obj)
        {
            var formData = ToKeyValue(obj);
            return new FormUrlEncodedContent(formData);
        }

        public static IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            // Added by me: avoid cyclic references
            var serializer = new Newtonsoft.Json.JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            JToken token = metaToken as JToken;
            if (token == null)
            {
                // Modified by me: use serializer defined above
                return ToKeyValue(JObject.FromObject(metaToken, serializer));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => ToCamelCase(k.Key), v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                            jValue?.ToString("o", CultureInfo.InvariantCulture) :
                            jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        private static string ToCamelCase(string the_string)
        {
            if (the_string == null || the_string.Length < 2)
                return the_string;

            string[] words = the_string.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }
    }
}
