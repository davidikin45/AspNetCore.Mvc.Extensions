using AspNetCore.Mvc.Extensions.Validation.Settings;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class EmailSettings : IValidateSettings
    {
        [Required]
        public string ToDisplayName { get; set; } = "Admin";
        [Required]
        public string FromDisplayName { get; set; } = "Web App";
        [Required]
        public string FromEmail { get; set; }
        [Required]
        public string ToEmail { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 25;
        public bool Ssl { get; set; } = true;
        public bool SendEmailsLive { get; set; } = false;
        public string SendGridApiKey { get; set; } = "";
        public bool WriteEmailsToFileSystem { get; set; } = true;
        [Required]
        public string FileSystemFolder { get; set; } = "emails";


    }
}
