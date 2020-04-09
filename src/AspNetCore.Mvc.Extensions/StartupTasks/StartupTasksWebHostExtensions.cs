using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public static class StartupTasksWebHostExtensions
    {
        public static Task RunStartupTasksAsync(this IWebHost host, CancellationToken cancellationToken)
        {
            return host.Services.RunStartupTasksAsync(cancellationToken);
        }

        public static Task RunStartupTasksAsync(this IHost host, CancellationToken cancellationToken)
        {
            return host.Services.RunStartupTasksAsync(cancellationToken);
        }
    }
}
