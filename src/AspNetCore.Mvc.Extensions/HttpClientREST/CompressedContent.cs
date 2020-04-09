using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HttpClientREST
{
    public enum CompressionMethod
    {
        GZip = 1,
        Deflate = 2,
        Br = 3
    }

    public class CompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;
        private readonly CompressionMethod _compressionMethod;

        public CompressedContent(HttpContent content, CompressionMethod compressionMethod)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            _originalContent = content;
            _compressionMethod = compressionMethod;

            foreach (KeyValuePair<string, IEnumerable<string>> header in _originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            Headers.ContentEncoding.Add(_compressionMethod.ToString().ToLowerInvariant());
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            if (_compressionMethod == CompressionMethod.GZip)
            {
                using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
                {
                    await _originalContent.CopyToAsync(gzipStream);
                }
            }
            else if (_compressionMethod == CompressionMethod.Deflate)
            {
                using (var deflateStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true))
                {
                    await _originalContent.CopyToAsync(deflateStream);
                }
            }
            else if (_compressionMethod == CompressionMethod.Br)
            {
                using (var brotliStream = new BrotliStream(stream, CompressionMode.Compress, leaveOpen: true))
                {
                    await _originalContent.CopyToAsync(brotliStream);
                }
            }
        }
    }
}
