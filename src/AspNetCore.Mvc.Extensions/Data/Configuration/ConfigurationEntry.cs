using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Data.Configuration
{
    public class ConfigurationEntry
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
