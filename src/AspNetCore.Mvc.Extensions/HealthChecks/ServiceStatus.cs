using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.HealthChecks
{
    public class ServiceStatus
    {
        public string Service { get; set; }
        public int Status { get; set; }

        public string StatusDescription { get; set; }
        public string Duration { get; set; }
        public string Exception { get; set; }
        public List<KeyValuePair<string,object>> Data { get; set; }
    }
}
