using AspNetCore.Mvc.Extensions.Validation.Settings;
using Microsoft.Extensions.Options;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class ElasticSettings : IValidateSettings
    {
        public string Uri { get; set; }
        public string DefaultIndex { get; set; } = "default";
        public bool Log { get; set; } = false;
    }
}
