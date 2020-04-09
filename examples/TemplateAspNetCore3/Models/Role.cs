using System.ComponentModel.DataAnnotations;

namespace TemplateAspNetCore3.Models
{
    public enum Role
    {
        [Display(Name = "Anonymous")]
        anonymous,
        [Display(Name = "Authenticated")]
        authenticated,
        [Display(Name = "Read-Only")]
        read_only,
        [Display(Name = "Admin")]
        admin
    }
}
