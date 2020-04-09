using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    //https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-2/
    //https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-3-feedback/
    //https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-4-using-health-checks/

    //https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-3/
    //This allows initialization tasks to run AFTER IStartupFilters have run and the middleware pipeline has been configured.

    //1. Tasks (Db Migration, Loading Cache, Configuration Validation) should be run asynchronously(i.e. using async/await), avoiding sync-over-async.
    //2. The DI container(and preferably the middleware pipeline) should be built before the tasks are run.
    //3. All tasks should be completed before Kestrel starts serving requests.

    //With Health Checks
    //4. Kestrel is started and can start handling requests before the tasks are started, but it should respond to all non-health-check traffic with a 503 response.
    //5. Health checks should only return "Healthy" once all startup tasks have completed.

    //In 3.0 can just use IStartupTask : IHostedService

    //This blocks 
    public class TaskExecutingServer : IServer
    {
        // Inject the original IServer implementation (KestrelServer/IISHttpServer)
        private readonly IServer _server;
        private readonly IServiceProvider _serviceProvider;
        private StartupTasksHostedService _startupTaskExecutor;
        public TaskExecutingServer(IServer server, IServiceProvider serviceProvider)
        {
            _server = server;
            _serviceProvider = serviceProvider;
        }

        //In .Net Core 3.0 IHostedServices run before StartAsync.
        public async Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            // Run the tasks first
            _startupTaskExecutor = _serviceProvider.GetRequiredService<StartupTasksHostedService>();
            await _startupTaskExecutor.StartAsync(cancellationToken);

            // Now start the Kestrel server properly
            await _server.StartAsync(application, cancellationToken);
        }

        // Delegate implementation to default IServer
        public IFeatureCollection Features => _server.Features;
        public void Dispose() => _server.Dispose();
        public async Task StopAsync(CancellationToken cancellationToken) {

            await _server.StopAsync(cancellationToken);

            if (_startupTaskExecutor != null)
            {
                await _startupTaskExecutor.StopAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public Type ServerType() => _server?.GetType();
    }
}
