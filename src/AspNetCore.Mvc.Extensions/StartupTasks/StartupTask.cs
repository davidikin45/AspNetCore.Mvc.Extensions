﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public abstract class StartupTaskBlocking : IStartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        public StartupTaskBlocking(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public abstract int Order { get; }

        protected abstract Task ExecuteAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken);
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                await ExecuteAsync(scope.ServiceProvider, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public abstract class StartupTaskNonBlocking : BackgroundService, IStartupTask
    {
        public virtual int Order { get; }


    }
}
