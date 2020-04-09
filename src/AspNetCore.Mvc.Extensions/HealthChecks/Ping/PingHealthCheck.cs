using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HealthChecks.Ping
{
    public class PingHealthCheck : IHealthCheck
    {
        private string _host;
        private int _timeout;
        private int _pingInterval;
        private DateTime _lastPingTime = DateTime.MinValue;
        private HealthCheckResult _lastPingResult = HealthCheckResult.Healthy();
        private object _locker = new object();


        public PingHealthCheck(string host, int timeout, int pingInterval = 0)
        {
            _host = host;
            _timeout = timeout;
            _pingInterval = pingInterval;
        }

        private bool IsCacheExpired()
        {
            return (_pingInterval == 0 || _lastPingTime.AddSeconds(_pingInterval) <= DateTime.Now);
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!IsCacheExpired())
            {
                return Task.FromResult(_lastPingResult);
            }

            if (Monitor.TryEnter(_locker))
            {
                try
                {
                    if (IsCacheExpired())
                    {
                        PingService();
                    }
                }
                finally
                {
                    Monitor.Exit(_locker);
                }
            }

            return Task.FromResult(_lastPingResult);
        }

        private void PingService()
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    _lastPingTime = DateTime.Now;

                    var reply = ping.Send(_host, _timeout);

                    if (reply.Status != IPStatus.Success)
                    {
                        _lastPingResult = HealthCheckResult.Unhealthy();
                    }
                    else if (reply.RoundtripTime >= _timeout)
                    {
                        _lastPingResult = HealthCheckResult.Degraded();
                    }
                    else
                    {
                        _lastPingResult = HealthCheckResult.Healthy();
                    }
                }
            }
            catch
            {
                _lastPingResult = HealthCheckResult.Unhealthy();
            }
        }
    }
}
