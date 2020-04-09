using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class AuthorizationSettings : IValidateSettings
    {
        public bool UserMustBeAuthorizedByDefault { get; set; } = false;
    }
}
