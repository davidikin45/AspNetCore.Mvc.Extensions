using AspNetCore.Mvc.Extensions.Validation.Settings;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class PasswordSettings : IValidateSettings
    {
        [Required]
        public string PasswordResetCallbackUrl { get; set; }
        public int MaxFailedAccessAttemptsBeforeLockout { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 10;
        public bool RequireDigit { get; set; } = true;
        public int RequiredLength { get; set; } = 8;
        public int RequiredUniqueChars { get; set; } = 4;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
    }
}
