using Microsoft.Net.Http.Headers;

namespace AspNetCore.Mvc.Extensions.Attributes.Routing
{
    public class AcceptHeaderMatchesMediaTypeAttribute : RequestHeaderMatchesMediaTypeAttribute
    {
        public AcceptHeaderMatchesMediaTypeAttribute(string mediaType, params string[] otherMediaTypes)
            : base(HeaderNames.Accept, mediaType, otherMediaTypes)
        {
        }
    }
}
