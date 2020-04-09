using AspNetCore.Mvc.Extensions.Validation.Settings;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class EmailTemplates : IValidateSettings
    {
        [Required]
        public string Welcome { get; set; } = "email_templates\\welcome.html";

        [Required]
        public string ResetPassword { get; set; } = "email_templates\\reset-password.html";
    }
}
