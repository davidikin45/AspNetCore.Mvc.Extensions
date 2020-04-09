using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class SwitchSettings : IValidateSettings
    {
        public bool EnableCookieConsent { get; set; } = false;
        public bool EnableHsts { get; set; } = false;
        public bool EnableIFramesGlobal { get; set; } = true;
        public bool EnableRedirectHttpToHttps { get; set; } = false;
        public bool EnableRedirectNonWwwToWww { get; set; } = true;
        public bool EnableMVCModelValidation { get; set; } = true;
        public bool EnableHelloWorld { get; set; } = false;
        public bool EnableSwagger { get; set; } = true;
        public bool EnableResponseCompression { get; set; } = false;
        public bool EnableIpRateLimiting { get; set; } = false;
        public bool EnableCors { get; set; } = true;
        public bool EnableResponseCaching { get; set; } = true;
        public bool EnableSignalR { get; set; } = true;
        public bool EnableETags { get; set; } = false;
        public bool EnableHangfire { get; set; } = true;
        public bool EnableClientValidation { get; set; } = true;
        public bool EnableApiXSRFTokenGeneration { get; set; } = true;
    }
}
