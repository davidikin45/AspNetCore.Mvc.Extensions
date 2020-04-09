using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class UserSettings : IValidateSettings
    {
        public bool RequireConfirmedEmail { get; set; } = false;
        public bool RequireUniqueEmail { get; set; } = false;
        public int RegistrationEmailConfirmationExprireDays { get; set; } = 2;
        public int ForgotPasswordEmailConfirmationExpireHours { get; set; } = 3;
        public int UserDetailsChangeLogoutMinutes { get; set; } = 5;
    }
}
