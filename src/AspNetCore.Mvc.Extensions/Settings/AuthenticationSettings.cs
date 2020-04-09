using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class AuthenticationSettings : IValidateSettings
    {
        public Authentication Basic { get; set; } = new Authentication() { Enable = true };
        public Authentication Application { get; set; } = new Authentication() { Enable = true };
        public Authentication JwtToken { get; set; } = new Authentication() { Enable = true };
        public Authentication OpenIdConnect { get; set; } = new Authentication() { Enable = false };
        public Authentication OpenIdConnectJwtToken { get; set; } = new Authentication() { Enable = false };
        public Authentication Google { get; set; } = new Authentication() { Enable = false };
        public Authentication Facebook { get; set; } = new Authentication() { Enable = false };
    }

    public class Authentication
    {
        public bool Enable { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
