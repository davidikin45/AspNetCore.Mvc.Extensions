using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace GrabMobile.ApiClient.HttpClientREST
{
    public class JsonContent : StringContent
    {
        public JsonContent(object value, JsonSerializerSettings serializerSettings)
           : base(JsonConvert.SerializeObject(value, serializerSettings), Encoding.UTF8, "application/json")
        {
        }

        public JsonContent(object value)
            : base(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json")
        {
        }

        public JsonContent(object value, string mediaType)
            : base(JsonConvert.SerializeObject(value), Encoding.UTF8, mediaType)
        {
        }
    }
}
