using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AspNetCore.Base.ModelBinders
{           
    //[FromBody] or [ApiController]
    //https://stackoverflow.com/questions/31952002/asp-net-core-mvc-how-to-get-raw-json-bound-to-a-string-without-a-type
    //options.InputFormatters.Insert(0, new RawStringRequestBodyInputFormatter());
    //options.InputFormatters.Insert(0, new RawBytesRequestBodyInputFormatter());

    public class RawStringRequestBodyInputFormatter : InputFormatter
    {
        public RawStringRequestBodyInputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!CanReadType(context.ModelType))
            {
                return false;
            }

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType))
                return true;

            return IsSubsetOfAnySupportedContentType(contentType);
        }

        private bool IsSubsetOfAnySupportedContentType(string contentType)
        {
            var parsedContentType = new MediaType(contentType);
            for (var i = 0; i < SupportedMediaTypes.Count; i++)
            {
                var supportedMediaType = new MediaType(SupportedMediaTypes[i]);
                if (parsedContentType.IsSubsetOf(supportedMediaType))
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }
    }

    public class RawStringRequestBodyInputFormatterMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public RawStringRequestBodyInputFormatterMvcOptionsSetup(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Configure(MvcOptions options)
        {
            options.InputFormatters.Insert(0, new RawStringRequestBodyInputFormatter());
        }
    }

    public class RawBytesRequestBodyInputFormatter : InputFormatter
    {
        public RawBytesRequestBodyInputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;

            using (var ms = new MemoryStream(2048))
            {
                await request.Body.CopyToAsync(ms);
                var content = ms.ToArray();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(byte[]);
        }
    }

    public class RawBytesRequestBodyInputFormatterMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public RawBytesRequestBodyInputFormatterMvcOptionsSetup(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Configure(MvcOptions options)
        {
            options.InputFormatters.Insert(0, new RawBytesRequestBodyInputFormatter());
        }
    }
}
