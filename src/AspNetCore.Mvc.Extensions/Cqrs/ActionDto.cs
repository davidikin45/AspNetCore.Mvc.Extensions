using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AspNetCore.Cqrs
{
    public class ActionDto : ActionDto<dynamic>
    {
 
    }

    public class ActionDto<T>
    {
        [Required]
        public string Type { get; set; }
        public T Payload { get; set; }
    }
}
