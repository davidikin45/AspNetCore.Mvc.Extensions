using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public interface IScopedProcessingService
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
