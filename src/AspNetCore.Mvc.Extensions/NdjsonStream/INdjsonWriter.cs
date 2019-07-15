using System;
using System.Threading.Tasks;


namespace AspNetCore.Mvc.Extensions.NdjsonStream
{
    //https://www.tpeczek.com/2019/04/fetch-api-streams-api-ndjson-and-aspnet.html?fbclid=IwAR1C3yeg-xczWA519EPdqvljU4JacPuN_K9Xrhyn6taQn0HEk9QVOYc7raM
    //https://github.com/tpeczek/Demo.AspNetCore.Mvc.FetchStreaming
    internal interface INdjsonWriter : IDisposable
    {
        Task WriteAsync(object value);
    }
}
