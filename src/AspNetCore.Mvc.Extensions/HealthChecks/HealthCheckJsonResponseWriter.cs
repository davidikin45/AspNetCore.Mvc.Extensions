using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HealthChecks
{
    public static class HealthCheckJsonResponseWriter
    {
        public static Task WriteResponse(HttpContext c, HealthReport r)
        {
            c.Response.ContentType = "application/json";
            var result = new List<ServiceStatus>();
            result.Add(new ServiceStatus { Service = "OverAll", Status = (int)r.Status, StatusDescription = r.Status.ToString(), Duration = r.TotalDuration.TotalSeconds.ToString("0:0.00") });
            result.AddRange(
                r.Entries.Select(
                    e => new ServiceStatus
                    {
                        Service = e.Key,
                        Status = (int)e.Value.Status,
                        StatusDescription = e.Value.Status.ToString(),
                        Data = e.Value.Data.Select(k => k).ToList(),
                        Duration = e.Value.Duration.TotalSeconds.ToString("0:0.00"),
                        Exception = e.Value.Exception?.Message
                    }
                )
            );

            var json = JsonConvert.SerializeObject(result);

            return c.Response.WriteAsync(json);
        }
    }
}
