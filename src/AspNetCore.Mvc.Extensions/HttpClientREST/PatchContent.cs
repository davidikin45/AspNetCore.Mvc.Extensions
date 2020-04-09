using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace GrabMobile.ApiClient.HttpClientREST
{
    public class PatchContent : StringContent
    {
        public PatchContent(object value, JsonSerializerSettings serializerSettings)
            : base(JsonConvert.SerializeObject(value, serializerSettings), Encoding.UTF8, "application/json-patch+json")
        {
        }

        public PatchContent(object value)
            : base(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json-patch+json")
        {
        }
    }
}
