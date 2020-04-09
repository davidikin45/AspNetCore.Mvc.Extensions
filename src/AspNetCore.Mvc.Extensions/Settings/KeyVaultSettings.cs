using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class KeyVaultSettings : IValidateSettings
    {
        public string Name { get; set; }
    }
}
