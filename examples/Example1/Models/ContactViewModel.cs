using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Example1.Models
{
    public class ContactViewModel
    {
        [Required]
        public string FirstName { get; set; }
    }
}
