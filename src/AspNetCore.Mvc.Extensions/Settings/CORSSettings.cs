using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class CORSSettings : IValidateSettings
    {
        public string[] Domains { get; set; } = new string[] { "" };
    }
}
