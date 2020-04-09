﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.NdjsonStream
{

    internal class NdjsonWriterFactory : INdjsonWriterFactory
    {
        private class NdjsonWriter : INdjsonWriter
        {
            private static byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");

            private readonly Stream _writeStream;

            private readonly JsonSerializerOptions _jsonSerializerOptions;

            public NdjsonWriter(Stream writeStream, JsonSerializerOptions jsonSerializerOptions)
            {
                _writeStream = writeStream;
                _jsonSerializerOptions = jsonSerializerOptions;
            }

            public async Task WriteAsync(object value)
            {
                Type valueType = value?.GetType() ?? typeof(object);

                await JsonSerializer.SerializeAsync(_writeStream, value, valueType, _jsonSerializerOptions);
                await _writeStream.WriteAsync(_newlineDelimiter, 0, _newlineDelimiter.Length);
                await _writeStream.FlushAsync();
            }

            public void Dispose()
            { }
        }

        private static readonly string CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private readonly MvcOptions _options;

        public NdjsonWriterFactory(IOptions<MvcOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public INdjsonWriter CreateWriter(ActionContext context, NdjsonStreamResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            HttpResponse response = context.HttpContext.Response;

            response.ContentType = CONTENT_TYPE;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            DisableResponseBuffering(context.HttpContext);

            return new NdjsonWriter(response.Body, null);
        }

        private static void DisableResponseBuffering(HttpContext context)
        {
            IHttpResponseBodyFeature bufferingFeature = context.Features.Get<IHttpResponseBodyFeature>();
            if (bufferingFeature != null)
            {
                bufferingFeature.DisableBuffering();
            }

            //.NET Core 2.2 
            //IHttpBufferingFeature bufferingFeature = context.Features.Get<IHttpBufferingFeature>();
            //if (bufferingFeature != null)
            //{
            //    bufferingFeature.DisableResponseBuffering();
            //}
        }
    }
}
