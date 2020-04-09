using AspNetCore.Mvc.Extensions.HttpClientREST;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrabMobile.ApiClient.HttpClientREST
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> Get(this HttpClient client, string requestUri, CancellationToken cancellationToken = default(CancellationToken))
           => await Get(client, requestUri, "", cancellationToken);

        public static async Task<HttpResponseMessage> Get(this HttpClient client, string requestUri, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Get)
                                .AddRequestUri(requestUri)
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        //https://weblog.west-wind.com/posts/2014/jan/29/using-net-httpclient-to-capture-partial-responses#Other-Posts-you-might-also-like
        public static async Task<byte[]> GetBytes(this HttpClient client, string requestUri, int length, CancellationToken cancellationToken = default(CancellationToken))
           => await GetBytes(client, requestUri, length, "", cancellationToken);

        public static async Task<byte[]> GetBytes(this HttpClient client, string requestUri, int length, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Get)
                                .AddRequestUri(requestUri)
                                .AddBytes(length)
                                .AddBearerToken(bearerToken);

            var response = await builder.SendAsync(client, cancellationToken, HttpCompletionOption.ResponseHeadersRead);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var bytes = new byte[length];
                var bytesread = stream.Read(bytes, 0, bytes.Length);
                stream.Close();
                return bytes;
            }
        }

        public static async Task<HttpResponseMessage> GetWithQueryString(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken = default(CancellationToken))
           => await GetWithQueryString(client, requestUri, value, "", cancellationToken);

        public static async Task<HttpResponseMessage> GetWithQueryString(this HttpClient client, string requestUri, object value, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Get)
                                .AddRequestUri(QueryHelpers.AddQueryString(requestUri, QueryStringHelper.ToKeyValue(value) ?? new Dictionary<string, string>()))
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<byte[]> GetBytesWithQueryString(this HttpClient client, string requestUri, int length, object value, CancellationToken cancellationToken = default(CancellationToken))
           => await GetBytesWithQueryString(client, requestUri, length, value, "", cancellationToken);

        public static async Task<byte[]> GetBytesWithQueryString(this HttpClient client, string requestUri, int length, object value, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Get)
                                .AddBytes(length)
                                .AddRequestUri(QueryHelpers.AddQueryString(requestUri, QueryStringHelper.ToKeyValue(value) ?? new Dictionary<string, string>()))
                                .AddBearerToken(bearerToken);

            var response = await builder.SendAsync(client, cancellationToken, HttpCompletionOption.ResponseHeadersRead);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var bytes = new byte[length];
                var bytesread = stream.Read(bytes, 0, bytes.Length);
                stream.Close();
                return bytes;
            }
        }

        public static async Task<HttpResponseMessage> Post(this HttpClient client, string requestUri, object value, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default(CancellationToken))
            => await Post(client, requestUri, value, serializerSettings, "", cancellationToken);

        public static async Task<HttpResponseMessage> Post(this HttpClient client,
            string requestUri, object value, JsonSerializerSettings serializerSettings, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Post)
                                .AddRequestUri(requestUri)
                                .AddContent(new JsonContent(value, serializerSettings))
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<HttpResponseMessage> PostAsStream(this HttpClient client, string requestUri, object value, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default(CancellationToken))
           => await PostAsStream(client, requestUri, value, serializerSettings, "", cancellationToken);

        public static async Task<HttpResponseMessage> PostAsStream(this HttpClient client,
            string requestUri, object value, JsonSerializerSettings serializerSettings, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, new UTF8Encoding(), 1024, true))
            {
                using (var tw = new JsonTextWriter(sw))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(tw, value);
                    tw.Flush();
                }
            }

            ms.Seek(0, SeekOrigin.Begin);

            using (var streamContent = new StreamContent(ms))
            {
                var builder = new HttpRequestBuilder()
                    .AddMethod(HttpMethod.Post)
                    .AddRequestUri(requestUri)
                    .AddContent(streamContent)
                    .AddBearerToken(bearerToken);

                return await builder.SendAsync(client, cancellationToken);
            }
        }

        public static async Task<HttpResponseMessage> PostForm(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken = default(CancellationToken))
           => await PostForm(client, requestUri, value, "", cancellationToken);

        public static async Task<HttpResponseMessage> PostForm(this HttpClient client,
            string requestUri, object value, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Post)
                                .AddRequestUri(requestUri)
                                .AddContent(FormUrlEncodedContentExtensions.FromObject(value))
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<HttpResponseMessage> Put(this HttpClient client, string requestUri, object value, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default(CancellationToken))
            => await Put(client, requestUri, value, serializerSettings, "", cancellationToken);

        public static async Task<HttpResponseMessage> Put(this HttpClient client,
            string requestUri, object value, JsonSerializerSettings serializerSettings, string bearerToken, CancellationToken cancellationToken = default(CancellationToken), int timeoutSeconds = 0)
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Put)
                                .AddRequestUri(requestUri)
                                .AddContent(new JsonContent(value, serializerSettings))
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<HttpResponseMessage> Patch(this HttpClient client, string requestUri, object value, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default(CancellationToken))
            => await Patch(client, requestUri, value, serializerSettings, "", cancellationToken);

        public static async Task<HttpResponseMessage> Patch(this HttpClient client, string requestUri, object value, JsonSerializerSettings serializerSettings, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(new HttpMethod("PATCH"))
                                .AddRequestUri(requestUri)
                                .AddContent(new PatchContent(value, serializerSettings))
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<HttpResponseMessage> Delete(this HttpClient client, string requestUri, CancellationToken cancellationToken = default(CancellationToken))
            => await Delete(client, requestUri, "", cancellationToken);

        public static async Task<HttpResponseMessage> Delete(this HttpClient client, string requestUri, object value, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Delete)
                                .AddRequestUri(requestUri)
                                .AddContent(new JsonContent(value, serializerSettings));

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<HttpResponseMessage> Delete(this HttpClient client,
            string requestUri, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Delete)
                                .AddRequestUri(requestUri)
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }

        public static async Task<HttpResponseMessage> PostFile(this HttpClient client, string requestUri,
            string filePath, string apiParamName, CancellationToken cancellationToken = default(CancellationToken), int timeoutSeconds = 0)
            => await PostFile(client, requestUri, filePath, apiParamName, "", cancellationToken);

        public static async Task<HttpResponseMessage> PostFile(this HttpClient client, string requestUri,
            string filePath, string apiParamName, string bearerToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var builder = new HttpRequestBuilder()
                                .AddMethod(HttpMethod.Post)
                                .AddRequestUri(requestUri)
                                .AddContent(new FileContent(filePath, apiParamName))
                                .AddBearerToken(bearerToken);

            return await builder.SendAsync(client, cancellationToken);
        }
    }
}
