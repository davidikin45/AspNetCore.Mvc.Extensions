using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public class DbInitializeStartupTask<TDbContext> : DbStartupTaskBlocking
        where TDbContext : class
    {
        public override int Order { get; }
        private readonly Action<IServiceProvider, TDbContext> _configure;

        public DbInitializeStartupTask(IServiceProvider serviceProvider, Action<IServiceProvider, TDbContext> configure, int order = 0)
            :base(serviceProvider)
        {
            Order = 0;
            _configure = configure;
        }

        protected override Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken)
        {
            var context = scopedServiceProvider.GetRequiredService<TDbContext>();
            _configure(scopedServiceProvider, context);
            return Task.CompletedTask;
        }
    }
}
