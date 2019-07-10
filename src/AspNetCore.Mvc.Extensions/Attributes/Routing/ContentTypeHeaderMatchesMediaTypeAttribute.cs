using Microsoft.Net.Http.Headers;

namespace AspNetCore.Mvc.Extensions.Attributes.Routing
{
    public class ContentTypeHeaderMatchesMediaTypeAttribute : RequestHeaderMatchesMediaTypeAttribute
    {
        public ContentTypeHeaderMatchesMediaTypeAttribute(string mediaType, params string[] otherMediaTypes)
            : base(HeaderNames.ContentType, mediaType, otherMediaTypes)
        {
        }
    }
}
