using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class TokenSettings : IValidateSettings
    {
        public int ExpiryMinutes { get; set; } = 60;
        public string Key { get; set; }
        public string PublicCertificatePath { get; set; }
        public string PrivateCertificatePath { get; set; }
        public string PrivateCertificatePasword { get; set; }
        public string PublicKeyPath { get; set; }
        public string PrivateKeyPath { get; set; }
        public string LocalIssuer { get; set; } = "https://localhost:44372";
        public string ExternalIssuers { get; set; }
        public string Audiences { get; set; } = "api";
    }
}
