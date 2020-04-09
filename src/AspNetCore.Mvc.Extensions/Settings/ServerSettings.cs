using AspNetCore.Mvc.Extensions.Validation.Settings;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class ServerSettings : IValidateSettings
    {
        [Required]
        public string ServerName { get; set; } = "webapp";

        [Required]
        public string[] ServerNames { get; set; } = new string[] { "webapp" }; 
    }
}
