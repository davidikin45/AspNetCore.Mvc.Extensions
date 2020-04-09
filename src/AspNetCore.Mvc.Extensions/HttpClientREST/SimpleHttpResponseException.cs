using System;
using System.Net;

namespace AspNetCore.Mvc.Extensions.HttpClientREST
{
    public class SimpleHttpResponseException : Exception
    {
        public int StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }

        public SimpleHttpResponseException(int statusCode, string reasonPhrase, string content) : base(content)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }
    }
}
