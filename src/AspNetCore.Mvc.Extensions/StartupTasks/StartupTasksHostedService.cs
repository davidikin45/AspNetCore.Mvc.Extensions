using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    //Blocking
    public class StartupTasksHostedService : IHostedService
    {
        private readonly ILogger<StartupTasksHostedService> _logger;
        private readonly IEnumerable<IDbStartupTask> _dbTasks;
        private readonly IEnumerable<IStartupTask> _tasks;
        private readonly StartupTaskContext _context;

        public StartupTasksHostedService(ILogger<StartupTasksHostedService> logger, IEnumerable<IDbStartupTask> dbTasks, IEnumerable<IStartupTask> tasks, StartupTaskContext context)
        {
            _logger = logger;
            _dbTasks = dbTasks;
            _tasks = tasks;
            _context = context;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunDbStartupTasksAsync(cancellationToken);
            await RunStartupTasksAsync(cancellationToken);
        }

        //HostedServiceExecutor starts all regardless of exceptions!
        //https://github.com/aspnet/AspNetCore/blob/f3f9a1cdbcd06b298035b523732b9f45b1408461/src/Hosting/Hosting/src/Internal/HostedServiceExecutor.cs 
        private async Task RunDbStartupTasksAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting db initialization");

            try
            {
                foreach (var task in _dbTasks.ToList().OrderBy(t => t.Order))
                {
                    _logger.LogInformation("Starting initialization for {InitializerType}", task.GetType());
                    try
                    {
                        await task.StartAsync(cancellationToken);
                        _context.MarkTaskAsComplete();
                        _logger.LogInformation("Initialization for {InitializerType} completed", task.GetType());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Initialization for {InitializerType} failed", task.GetType());
                        throw;
                    }
                }

                _logger.LogInformation("db initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "db initialization failed");
                throw;
            }
        }

        private async Task RunStartupTasksAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting initialization");

            try
            {
                foreach (var task in _tasks.ToList().OrderBy(t => t.Order))
                {
                    _logger.LogInformation("Starting initialization for {InitializerType}", task.GetType());
                    try
                    {
                        await task.StartAsync(cancellationToken);
                        _context.MarkTaskAsComplete();
                        _logger.LogInformation("Initialization for {InitializerType} completed", task.GetType());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Initialization for {InitializerType} failed", task.GetType());
                        throw;
                    }
                }

                _logger.LogInformation("Initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initialization failed");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            List<Exception> exceptions = null;

            try
            {
                await ExecuteDbTaskAsync(service => service.StopAsync(cancellationToken));
            }
            catch (AggregateException ex)
            {
                if (exceptions == null)
                {
                    exceptions = new List<Exception>();
                }

                exceptions.AddRange(ex.InnerExceptions);
            }

            try
            {
                await ExecuteTaskAsync(service => service.StopAsync(cancellationToken));
            }
            catch (AggregateException ex)
            {
                if (exceptions == null)
                {
                    exceptions = new List<Exception>();
                }

                exceptions.AddRange(ex.InnerExceptions);
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        private async Task ExecuteTaskAsync(Func<IStartupTask, Task> callback)
        {
            List<Exception> exceptions = null;

            foreach (var task in _tasks)
            {
                try
                {
                    await callback(task);
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        private async Task ExecuteDbTaskAsync(Func<IDbStartupTask, Task> callback)
        {
            List<Exception> exceptions = null;

            foreach (var task in _dbTasks)
            {
                try
                {
                    await callback(task);
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            // Throw an aggregate exception if there were any exceptions
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
