using Microsoft.AspNetCore.WebUtilities;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GrabMobile.ApiClient.HttpClientREST
{
    public class HttpRequestBuilder
    {
        private HttpMethod method = null;
        private string requestUri = "";
        private HttpContent content = null;
        private string bearerToken = "";
        private int? length;
        private string acceptHeader = "application/json";
        private string acceptEncodingHeader = "gzip";
        private bool allowAutoRedirect = false;

        public HttpRequestBuilder()
        {
        }

        public HttpRequestBuilder AddMethod(HttpMethod method)
        {
            this.method = method;
            return this;
        }

        public HttpRequestBuilder AddBytes(int length)
        {
            this.length = length;
            return this;
        }

        public HttpRequestBuilder AddRequestUri(string requestUri)
        {
            this.requestUri = requestUri;
            return this;
        }

        public HttpRequestBuilder AddContent(HttpContent content)
        {
            this.content = content;
            return this;
        }

        public HttpRequestBuilder AddBearerToken(string bearerToken)
        {
            this.bearerToken = bearerToken;
            return this;
        }

        public HttpRequestBuilder AddAcceptHeader(string acceptHeader)
        {
            this.acceptHeader = acceptHeader;
            return this;
        }

        public HttpRequestBuilder AddAcceptEncodingHeader(string acceptEncodingHeader)
        {
            this.acceptEncodingHeader = acceptEncodingHeader;
            return this;
        }

        public HttpRequestBuilder AddAllowAutoRedirect(bool allowAutoRedirect)
        {
            this.allowAutoRedirect = allowAutoRedirect;
            return this;
        }

        public async Task<HttpResponseMessage> SendAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = this.allowAutoRedirect;
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;

            var client = new HttpClient(handler);

            return await SendAsync(client, cancellationToken).ConfigureAwait(false);
        }

        //Throws OperationCanceledException on timeout
        public async Task<HttpResponseMessage> SendAsync(HttpClient client, CancellationToken cancellationToken = default(CancellationToken), HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
        {
            // Check required arguments
            EnsureArguments();

            var uri = this.requestUri;

            // Set up request
            using (var request = new HttpRequestMessage(this.method, uri))
            {
                if (this.content != null)
                    request.Content = this.content;

                if (!string.IsNullOrEmpty(this.bearerToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.bearerToken);

                request.Headers.Accept.Clear();
                if (!string.IsNullOrEmpty(this.acceptHeader))
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(this.acceptHeader));

                request.Headers.AcceptEncoding.Clear();
                if (!string.IsNullOrEmpty(this.acceptEncodingHeader))
                    request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(this.acceptEncodingHeader));

                if(length.HasValue)
                    request.Headers.Range = new RangeHeaderValue(0, length.Value);

                return await client.SendAsync(request, httpCompletionOption, cancellationToken);
            }
        }


        #region " Private "

        private void EnsureArguments()
        {
            if (this.method == null)
                throw new ArgumentNullException("Method");

            if (string.IsNullOrEmpty(this.requestUri))
                throw new ArgumentNullException("Request Uri");
        }

        #endregion
    }
}
