using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public static class StartupTasksServiceProviderExtensions
    {
        public static Task RunStartupTasksAsync(this IServiceProvider services, CancellationToken cancellationToken)
        {
            var startupTaskExecutor = services.GetRequiredService<StartupTasksHostedService>();
            return startupTaskExecutor.StartAsync(cancellationToken);
        }
    }
}
