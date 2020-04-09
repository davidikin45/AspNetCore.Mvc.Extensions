using AspNetCore.Mvc.Extensions.Validation.Settings;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class LocalizationSettings : IValidateSettings
    {
        [Required]
        public string DefaultCulture { get; set; } = "en-US";

        [Required]
        public string[] SupportedUICultures { get; set; } = new string[] { "en" };

        public bool SupportAllLanguagesFormatting { get; set; } = false;
        public bool SupportAllCountriesFormatting { get; set; } = false;
        public bool SupportUICultureFormatting { get; set; } = true;
        public bool SupportDefaultCultureLanguageFormatting { get; set; } = true;

        public bool AlwaysIncludeCultureInUrl { get; set; } = true;
    }
}
