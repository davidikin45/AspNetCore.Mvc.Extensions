using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Middleware
{
    public abstract class BaseRangeRequestHandler
    {
        #region Constants
        private const string MULTIPART_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0>";
        private const string MULTIPART_CONTENTTYPE = "multipart/byteranges; boundary=" + MULTIPART_BOUNDARY;
        private const string DEFAULT_CONTENTTYPE = "application/octet-stream";
        private const string HTTP_HEADER_ACCEPT_RANGES = "Accept-Ranges";
        private const string HTTP_HEADER_ACCEPT_RANGES_BYTES = "bytes";
        private const string HTTP_HEADER_ACCEPT_RANGES_NONE = "none";
        private const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
        private const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
        private const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        private const string HTTP_HEADER_CONTENT_DISPOSITION = "Content-Disposition";
        private const string HTTP_HEADER_ENTITY_TAG = "ETag";
        private const string HTTP_HEADER_LAST_MODIFIED = "Last-Modified";
        private const string HTTP_HEADER_RANGE = "Range";
        private const string HTTP_HEADER_IF_RANGE = "If-Range";
        private const string HTTP_HEADER_IF_MATCH = "If-Match";
        private const string HTTP_HEADER_IF_NONE_MATCH = "If-None-Match";
        private const string HTTP_HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";
        private const string HTTP_HEADER_IF_UNMODIFIED_SINCE = "If-Unmodified-Since";
        private const string HTTP_HEADER_UNLESS_MODIFIED_SINCE = "Unless-Modified-Since";
        private const string HTTP_METHOD_GET = "GET";
        private const string HTTP_METHOD_HEAD = "HEAD";

        private const int DEBUGGING_SLEEP_TIME = 0;
        #endregion

        protected BaseRangeRequestHandler()
        {
            ProcessRequestCheckSteps = new Func<HttpContext, bool>[]
                    {
                        CheckAuthorizationRules,
                        CheckHttpMethod,
                        CheckFileRequested,
                        CheckRangesRequested,
                        CheckIfModifiedSinceHeader,
                        CheckIfUnmodifiedSinceHeader,
                        CheckIfMatchHeader,
                        CheckIfNoneMatchHeader,
                    };
        }

        #region Properties
        /// <summary>
        /// Indicates if the HTTP request is for multiple ranges.
        /// </summary>
        public bool IsMultipartRequest { get; private set; }

        /// <summary>
        /// Indicates if the HTTP request is for one or more ranges.
        /// </summary>
        public bool IsRangeRequest { get; private set; }

        /// <summary>
        /// The start byte(s) for the requested range(s).
        /// </summary>
        public long[] StartRangeBytes { get; private set; }

        /// <summary>
        /// The end byte(s) for the requested range(s).
        /// </summary>
        public long[] EndRangeBytes { get; private set; }

        /// <summary>
        /// The size of each chunk of data streamed back to the client.
        /// </summary>
        /// <remarks>
        /// When a client makes a range request the requested file's contents are
        /// read in BufferSize chunks, with each chunk flushed to the output stream
        /// until the requested byte range has been read.
        /// </remarks>
        public virtual int BufferSize { get { return 10240; } }

        /// <summary>
        /// Indicates whether Range requests are enabled. If false, the HTTP Handler
        /// ignores the Range HTTP Header and returns the entire contents.
        /// </summary>
        public virtual bool EnableRangeRequests { get { return true; } }


        private Func<HttpContext, bool>[] ProcessRequestCheckSteps { get; set; }
        protected FileInfo InternalRequestedFileInfo { get; set; }
        private string InternalRequestedFileEntityTag { get; set; }
        private string InternalRequestedFileMimeType { get; set; }
        private readonly NameValueCollection _internalResponseHeaders = new NameValueCollection();
        #endregion

        /// <summary>
        /// Returns a FileInfo object representing the requested content.
        /// </summary>
        public abstract FileInfo GetRequestedFileInfo(HttpContext context);

        /// <summary>
        /// Returns the Entity Tag (ETag) for the requested content.
        /// </summary>
        /// <remarks>
        /// The Entity Tag is computed by taking the physical path to the file, concatenating it with the
        /// file's created date and time, and computing the MD5 hash of that string.
        /// 
        /// A derived class MAY override this method to return an Entity Tag
        /// value computed using an alternate approach.
        /// </remarks>
        public virtual string GetRequestedFileEntityTag(HttpContext context)
        {
            FileInfo requestedFile = GetRequestedFileInfo(context);
            if (requestedFile == null)
                return string.Empty;

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] sourceBytes = ascii.GetBytes(
                string.Concat(
                    requestedFile.FullName,
                    "|",
                    LastModifiedUtc(context)
                    )
                );

            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(sourceBytes));
        }

        /// <summary>
        /// Returns the MIME type for the requested content.
        /// </summary>
        /// <remarks>
        /// A dervied class SHOULD override this method and return the MIME type specific
        /// to the requested content. See http://www.iana.org/assignments/media-types/ for
        /// a list of MIME types registered with the Internet Assigned Numbers Authority (IANA).
        /// </remarks>
        public virtual string GetRequestedFileMimeType(HttpContext context)
        {
            return DEFAULT_CONTENTTYPE;
        }

        //protected CancellationToken ClientDisconnectedToken()
        //{
        //    return DND.Common.Infrastructure.HttpContext.Current.RequestAborted;
        //}

        public abstract void ParseRequestParameters(HttpContext context);

        public abstract Task CreateResponseContentIfRequiredAsync(HttpContext context);

        public abstract Stream GetResponseStream(HttpContext context);

        public virtual void HandleException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        public virtual bool TrySkipIisCustomErrors { get { return true; } }

        public virtual async Task ProcessRequestAsync(HttpContext context)
        {
            //context.Response.TrySkipIisCustomErrors = TrySkipIisCustomErrors;

            try
            {
                ParseRequestParameters(context);

                if (CheckAuthorizationRules(context) == false)
                {
                    return;
                }

                if (CheckRequestResourceExists(context) == false)
                {
                    return;
                }

                InternalRequestedFileInfo = GetRequestedFileInfo(context);

                InternalRequestedFileEntityTag = GetRequestedFileEntityTag(context);

               await CreateResponseContentIfRequiredAsync(context);

                InternalRequestedFileMimeType = GetRequestedFileMimeType(context);
                ParseRequestHeaderRanges(context);

                // Perform each check; exit if any check returns false
                if (ProcessRequestCheckSteps.Any(check => check(context) == false) || (await CheckIfRangeHeader(context) == false))
                {
                    return;
                }

                if (!EnableRangeRequests || !IsRangeRequest)
                    await ReturnEntireEntity(context);
                else
                    await ReturnPartialEntity(context);
            }
            catch (OperationCanceledException)
            {
                //do nothing
            }
            catch
            {
                //HandleException(context, ex);
            }

            //EndRequest(context);
        }

        private void EndRequest(HttpContext context)
        {
            context.Response.Body.Flush();
            //context.Response.SuppressContent = true;
            //HttpContext.Current.ApplicationInstance.CompleteRequest();
            Thread.Sleep(1);
        }

        protected abstract long GetResponseFileLength();

        private async Task ReturnEntireEntity(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.StatusCode = StatusCodes.Status200OK;  // OK
            WriteCommonResponseHeaders(response, GetResponseFileLength(), InternalRequestedFileMimeType, context);

            AddHeader(response, HTTP_HEADER_CONTENT_DISPOSITION, string.Format(((!DownloadFile(context)) ? "inline" : "attachment") + "; filename={0}", GetResponseFileName(context)));

            if (request.Method.Equals(HTTP_METHOD_HEAD) == false)
                //response.TransmitFile(InternalRequestedFileInfo.FullName);
                await ReturnChunkedResponse(context);
        }

        private async Task ReturnPartialEntity(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.StatusCode = StatusCodes.Status206PartialContent;  // Partial response

            // Specify the byte range being returned for non-multipart requests
            if (IsMultipartRequest == false)
                AddHeader(response, HTTP_HEADER_CONTENT_RANGE,
                          string.Format("bytes {0}-{1}/{2}", StartRangeBytes[0], EndRangeBytes[0], GetResponseFileLength()));

            WriteCommonResponseHeaders(response,
                                       ComputeContentLength(),
                                       IsMultipartRequest ? MULTIPART_CONTENTTYPE : InternalRequestedFileMimeType, context);

            if (request.Method.Equals(HTTP_METHOD_HEAD) == false)
                await ReturnChunkedResponse(context);
        }

        private async Task ReturnChunkedResponse(HttpContext context)
        {
            HttpResponse response = context.Response;
            // InternalRequestedFileInfo.OpenRead()
            byte[] buffer = new byte[BufferSize];
            using (Stream fs = GetResponseStream(context))
            {
                for (int i = 0; i < StartRangeBytes.Length; i++)
                {
                    // Position the stream at the starting byte
                    fs.Seek(StartRangeBytes[i], SeekOrigin.Begin);

                    int bytesToReadRemaining = Convert.ToInt32(EndRangeBytes[i] - StartRangeBytes[i]) + 1;

                    // Output multipart boundary, if needed
                    if (IsMultipartRequest)
                    {
                        await response.WriteAsync("--" + MULTIPART_BOUNDARY);
                        await response.WriteAsync(string.Format("{0}: {1}", HTTP_HEADER_CONTENT_TYPE, InternalRequestedFileMimeType));
                        await response.WriteAsync(string.Format("{0}: bytes {1}-{2}/{3}",
                                                            HTTP_HEADER_CONTENT_RANGE,
                                                            StartRangeBytes[i],
                                                            EndRangeBytes[i],
                                                            GetResponseFileLength()));
                        await response.WriteAsync(Environment.NewLine);
                    }

                    // Stream out the requested chunks for the current range request
                    while (bytesToReadRemaining > 0)
                    {
                        if (response.HttpContext.RequestAborted.IsCancellationRequested)
                            return;

                        int chunkSize = fs.Read(buffer, 0,
                                                BufferSize < bytesToReadRemaining
                                                    ? BufferSize
                                                    : bytesToReadRemaining);

                        await response.Body.WriteAsync(buffer, 0, chunkSize);

                        bytesToReadRemaining -= chunkSize;
                        if (response.HttpContext.RequestAborted.IsCancellationRequested)
                            return;
                        response.Body.Flush();
                        buffer = new Byte[BufferSize];
                        System.Threading.Thread.Sleep(DEBUGGING_SLEEP_TIME);
                    }
                }

                fs.Close();
            }
        }

        private int ComputeContentLength()
        {
            int contentLength = 0;

            for (int i = 0; i < StartRangeBytes.Length; i++)
            {
                contentLength += Convert.ToInt32(EndRangeBytes[i] - StartRangeBytes[i]) + 1;

                if (IsMultipartRequest)
                    contentLength += MULTIPART_BOUNDARY.Length
                                     + InternalRequestedFileMimeType.Length
                                     + StartRangeBytes[i].ToString().Length
                                     + EndRangeBytes[i].ToString().Length
                                     + GetResponseFileLength().ToString().Length
                                     + 49;       // Length needed for multipart header
            }

            if (IsMultipartRequest)
                contentLength += MULTIPART_BOUNDARY.Length + 8;    // Length of dash and line break

            return contentLength;
        }

        public abstract Boolean DownloadFile(HttpContext context);
        public abstract string GetResponseFileName(HttpContext context);

        public virtual DateTime LastModifiedUtc(HttpContext context)
        {
            return InternalRequestedFileInfo.LastWriteTimeUtc;
        }

        private void WriteCommonResponseHeaders(HttpResponse Response, long contentLength, string contentType, HttpContext context)
        {
            AddHeader(Response, HTTP_HEADER_CONTENT_LENGTH, contentLength.ToString());
            AddHeader(Response, HTTP_HEADER_CONTENT_TYPE, contentType);
            AddHeader(Response, HTTP_HEADER_LAST_MODIFIED, LastModifiedUtc(context).ToString("r"));
            AddHeader(Response, HTTP_HEADER_ENTITY_TAG, string.Concat("\"", InternalRequestedFileEntityTag, "\""));
            //AddHeader(Response, HTTP_HEADER_CONTENT_DISPOSITION, string.Format(((!DownloadFile(context)) ? "inline" : "attachment") + "; filename={0}", GetResponseFileName(context)));
            SetCache(context, CacheDays());

            if (EnableRangeRequests)
                AddHeader(Response, HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_BYTES);
            else
                AddHeader(Response, HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_NONE);
        }

        public abstract int CacheDays();

        private void SetCache(HttpContext context, int days)
        {
            if (days > 0)
            {
                TimeSpan timeSpan = new TimeSpan(days * 24, 0, 0);
                context.Response.GetTypedHeaders().Expires = DateTime.Now.Add(timeSpan).Date.ToUniversalTime();
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = timeSpan,

                };
            }
            else
            {
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                {
                    NoCache = true
                };
            }

            //Vary cache by query string
            //https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-2.1
            var responseCachingFeature = context.Response.HttpContext.Features.Get<IResponseCachingFeature>();
            if (responseCachingFeature != null)
            {
                responseCachingFeature.VaryByQueryKeys = new[] { "*" };
            }
        }

        private string RetrieveHeader(HttpRequest Request, string headerName, string defaultValue)
        {
            return string.IsNullOrEmpty(Request.Headers[headerName]) ? defaultValue : Request.Headers[headerName].ToString().Replace("\"", string.Empty);
        }

        protected virtual void ParseRequestHeaderRanges(HttpContext context)
        {
            HttpRequest request = context.Request;

            string rangeHeader = RetrieveHeader(request, HTTP_HEADER_RANGE, string.Empty);

            if (string.IsNullOrEmpty(rangeHeader))
            {
                // No Range HTTP Header supplied; send back entire contents
                StartRangeBytes = new long[] { 0 };
                EndRangeBytes = new long[] { GetResponseFileLength() - 1 };
                IsRangeRequest = false;
                IsMultipartRequest = false;
            }
            else
            {
                // rangeHeader contains the value of the Range HTTP Header and can have values like:
                //      Range: bytes=0-1            * Get bytes 0 and 1, inclusive
                //      Range: bytes=0-500          * Get bytes 0 to 500 (the first 501 bytes), inclusive
                //      Range: bytes=400-1000       * Get bytes 500 to 1000 (501 bytes in total), inclusive
                //      Range: bytes=-200           * Get the last 200 bytes
                //      Range: bytes=500-           * Get all bytes from byte 500 to the end
                //
                // Can also have multiple ranges delimited by commas, as in:
                //      Range: bytes=0-500,600-1000 * Get bytes 0-500 (the first 501 bytes), inclusive plus bytes 600-1000 (401 bytes) inclusive

                // Remove "Ranges" and break up the ranges
                string[] ranges = rangeHeader.Replace("bytes=", string.Empty).Split(",".ToCharArray());

                StartRangeBytes = new long[ranges.Length];
                EndRangeBytes = new long[ranges.Length];

                IsRangeRequest = true;
                IsMultipartRequest = (StartRangeBytes.Length > 1);

                for (int i = 0; i < ranges.Length; i++)
                {
                    const int START = 0, END = 1;

                    // Get the START and END values for the current range
                    string[] currentRange = ranges[i].Split("-".ToCharArray());

                    if (string.IsNullOrEmpty(currentRange[END]))
                        // No end specified
                        EndRangeBytes[i] = GetResponseFileLength() - 1;
                    else
                        // An end was specified
                        EndRangeBytes[i] = long.Parse(currentRange[END]);

                    if (string.IsNullOrEmpty(currentRange[START]))
                    {
                        // No beginning specified, get last n bytes of file
                        StartRangeBytes[i] = GetResponseFileLength() - 1 - EndRangeBytes[i];
                        EndRangeBytes[i] = GetResponseFileLength() - 1;
                    }
                    else
                    {
                        // A normal begin value
                        StartRangeBytes[i] = long.Parse(currentRange[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an HTTP Response Header
        /// </summary>
        /// <remarks>
        /// This method is used to store the Response Headers in a private, member variable,
        /// InternalResponseHeaders, so that the Response Headers may be accesed in the
        /// LogResponseHttpHeaders method, if needed. The Response.Headers property can only
        /// be accessed directly when using IIS 7's Integrated Pipeline mode. This workaround
        /// permits logging of Response Headers when using Classic mode or a web server other
        /// than IIS 7.
        /// </remarks>
        protected void AddHeader(HttpResponse response, string name, string value)
        {
            _internalResponseHeaders.Add(name, value);
            response.Headers.Add(name, value);
        }

        #region Process Request Step Checks
        public abstract bool IsAuthorized(HttpContext context);

        protected virtual bool CheckAuthorizationRules(HttpContext context)
        {
            if (!IsAuthorized(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return false;
            }
            return true;
        }

        public virtual bool RequestResourceExists(HttpContext context)
        {
            return InternalRequestedFileInfo != null;
        }

        protected virtual bool CheckRequestResourceExists(HttpContext context)
        {
            if (!RequestResourceExists(context))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return false;
            }
            return true;
        }

        protected virtual bool CheckHttpMethod(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            if (!request.Method.Equals(HTTP_METHOD_GET) &&
                !request.Method.Equals(HTTP_METHOD_HEAD))
            {
                response.StatusCode = StatusCodes.Status501NotImplemented;  // Not Implemented
                return false;
            }

            return true;
        }

        protected virtual bool CheckFileRequested(HttpContext context)
        {
            HttpResponse response = context.Response;

            if (!RequestResourceExists(context))
            {
                response.StatusCode = StatusCodes.Status404NotFound;   // Not Found
                return false;
            }

            if (GetResponseFileLength() > int.MaxValue)
            {
                response.StatusCode = StatusCodes.Status413RequestEntityTooLarge; // Request Entity Too Large
                return false;
            }

            return true;
        }

        protected virtual bool CheckRangesRequested(HttpContext context)
        {
            for (int i = 0; i < StartRangeBytes.Length; i++)
            {
                if (StartRangeBytes[i] > GetResponseFileLength() - 1 ||
                    EndRangeBytes[i] > GetResponseFileLength() - 1)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                    return false;
                }

                if (StartRangeBytes[i] < 0 || EndRangeBytes[i] < 0)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                    return false;
                }

                if (EndRangeBytes[i] >= StartRangeBytes[i])
                    continue;

                context.Response.StatusCode = StatusCodes.Status400BadRequest;  // Bad Request
                return false;
            }
            return true;
        }

        protected virtual bool CheckIfModifiedSinceHeader(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            string ifModifiedSinceHeader = RetrieveHeader(request, HTTP_HEADER_IF_MODIFIED_SINCE, string.Empty);

            if (!string.IsNullOrEmpty(ifModifiedSinceHeader))
            {
                // Determine the date
                DateTime ifModifiedSinceDate;
                DateTime.TryParse(ifModifiedSinceHeader, out ifModifiedSinceDate);

                if (ifModifiedSinceDate == DateTime.MinValue)
                    // Could not parse date... do not continue on with check
                    return true;

                DateTime requestedFileModifiedDate = LastModifiedUtc(context);
                requestedFileModifiedDate = new DateTime(
                    requestedFileModifiedDate.Year,
                    requestedFileModifiedDate.Month,
                    requestedFileModifiedDate.Day,
                    requestedFileModifiedDate.Hour,
                    requestedFileModifiedDate.Minute,
                    requestedFileModifiedDate.Second
                    );
                ifModifiedSinceDate = ifModifiedSinceDate.ToUniversalTime();

                if (requestedFileModifiedDate <= ifModifiedSinceDate)
                {
                    // File was created before specified date
                    SetCache(context, CacheDays());
                    response.StatusCode = StatusCodes.Status304NotModified;  // Not Modified
                    return false;
                }
            }

            return true;
        }

        protected virtual bool CheckIfUnmodifiedSinceHeader(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            string ifUnmodifiedSinceHeader = RetrieveHeader(request, HTTP_HEADER_IF_UNMODIFIED_SINCE, string.Empty);

            if (string.IsNullOrEmpty(ifUnmodifiedSinceHeader))
                // Look for Unless-Modified-Since header
                ifUnmodifiedSinceHeader = RetrieveHeader(request, HTTP_HEADER_UNLESS_MODIFIED_SINCE, string.Empty);

            if (!string.IsNullOrEmpty(ifUnmodifiedSinceHeader))
            {
                // Determine the date
                DateTime ifUnmodifiedSinceDate;
                DateTime.TryParse(ifUnmodifiedSinceHeader, out ifUnmodifiedSinceDate);

                DateTime requestedFileModifiedDate = LastModifiedUtc(context);
                requestedFileModifiedDate = new DateTime(
                    requestedFileModifiedDate.Year,
                    requestedFileModifiedDate.Month,
                    requestedFileModifiedDate.Day,
                    requestedFileModifiedDate.Hour,
                    requestedFileModifiedDate.Minute,
                    requestedFileModifiedDate.Second
                    );
                if (ifUnmodifiedSinceDate != DateTime.MinValue)
                    ifUnmodifiedSinceDate = ifUnmodifiedSinceDate.ToUniversalTime();

                if (requestedFileModifiedDate > ifUnmodifiedSinceDate)
                {
                    // Could not convert value into date or file was created after specified date
                    response.StatusCode = StatusCodes.Status412PreconditionFailed;  // Precondition failed
                    return false;
                }
            }

            return true;
        }

        protected virtual bool CheckIfMatchHeader(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            string ifMatchHeader = RetrieveHeader(request, HTTP_HEADER_IF_MATCH, string.Empty);

            if (string.IsNullOrEmpty(ifMatchHeader) || ifMatchHeader == "*")
                return true;        // Match found

            // Look for a matching ETag value in ifMatchHeader
            string[] entityIds = ifMatchHeader.Replace("bytes=", string.Empty).Split(",".ToCharArray());

            if (entityIds.Any(entityId => InternalRequestedFileEntityTag == entityId))
                return true; // Match found

            // If we reach here, no match found
            response.StatusCode = StatusCodes.Status412PreconditionFailed;  // Precondition failed
            return false;
        }

        protected virtual bool CheckIfNoneMatchHeader(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            string ifNoneMatchHeader = RetrieveHeader(request, HTTP_HEADER_IF_NONE_MATCH, string.Empty);

            if (string.IsNullOrEmpty(ifNoneMatchHeader))
                return true;

            if (ifNoneMatchHeader == "*")
            {
                // Logically invalid request
                response.StatusCode = StatusCodes.Status412PreconditionFailed;  // Precondition failed
                return false;
            }

            // Look for a matching ETag value in ifNoneMatchHeader
            string[] entityIds = ifNoneMatchHeader.Replace("bytes=", string.Empty).Split(",".ToCharArray());

            foreach (string entityId in entityIds.Where(entityId => InternalRequestedFileEntityTag == entityId))
            {
                AddHeader(response, HTTP_HEADER_ENTITY_TAG, string.Concat("\"", entityId, "\""));
                SetCache(context, CacheDays());
                response.StatusCode = StatusCodes.Status304NotModified;  // Not modified
                return false;        // Match found
            }

            // No match found
            return true;
        }

        protected async virtual Task<bool> CheckIfRangeHeader(HttpContext context)
        {
            HttpRequest request = context.Request;

            string ifRangeHeader = RetrieveHeader(request, HTTP_HEADER_IF_RANGE, InternalRequestedFileEntityTag);

            if (IsRangeRequest && ifRangeHeader != InternalRequestedFileEntityTag)
            {
                await ReturnEntireEntity(context);
                return false;
            }
            return true;
        }
        #endregion
    }
}
