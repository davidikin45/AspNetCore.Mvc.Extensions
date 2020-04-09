using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public abstract class StartupTaskBlocking : IStartupTask
    {
        public abstract int Order { get; }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public abstract class StartupTaskNonBlocking : BackgroundService, IStartupTask
    {
        public virtual int Order { get; }


    }
}
