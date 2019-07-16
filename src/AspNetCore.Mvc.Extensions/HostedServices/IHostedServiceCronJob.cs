using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public interface IHostedServiceCronJob
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
