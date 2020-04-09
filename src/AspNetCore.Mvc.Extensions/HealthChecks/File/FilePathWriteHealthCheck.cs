using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HealthChecks.File
{
    public class FilePathWriteHealthCheck : IHealthCheck
    {
        private string _filePath;
        private IReadOnlyDictionary<string, object> _HealthCheckData;

        public FilePathWriteHealthCheck(string filePath)
        {
            _filePath = filePath;
            _HealthCheckData = new Dictionary<string, object>
            {
                { "filePath", _filePath }
            };
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var testFile = $"{_filePath}\\test.txt";
                var fs = System.IO.File.Create(testFile);
                fs.Close();
                System.IO.File.Delete(testFile);

                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception e)
            {

                switch (context.Registration.FailureStatus)
                {
                    case HealthStatus.Degraded:
                        return Task.FromResult(HealthCheckResult.Degraded($"Issues writing to file path",
                            e, _HealthCheckData));
                    case HealthStatus.Healthy:
                        return Task.FromResult(HealthCheckResult.Healthy($"Issues writing to file path", _HealthCheckData));
                    default:
                        return Task.FromResult(HealthCheckResult.Unhealthy($"Issues writing to file path",
                            e, _HealthCheckData));
                }
            }
        }
    }
}
