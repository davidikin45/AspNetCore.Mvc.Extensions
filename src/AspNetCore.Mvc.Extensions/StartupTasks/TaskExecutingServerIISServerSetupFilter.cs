using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    //https://github.com/aspnet/IISIntegration/blob/370490d2a36d46a6714e3b5321ad8139561f4000/src/Microsoft.AspNetCore.Server.IIS/Core/IISServerSetupFilter.cs
    internal class TaskExecutingServerIISServerSetupFilter : IStartupFilter
    {
        private string _virtualPath;

        public TaskExecutingServerIISServerSetupFilter(string virtualPath)
        {
            _virtualPath = virtualPath;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var server = app.ApplicationServices.GetRequiredService<IServer>();
                if ((server?.GetType() != typeof(TaskExecutingServer)) || (server is TaskExecutingServer taskExecutingServer && taskExecutingServer.ServerType()?.Name != "IISHttpServer"))
                {
                    throw new InvalidOperationException("Application is running inside IIS process but is not configured to use IIS server.");
                }

                app.UsePathBase(_virtualPath);
                next(app);
            };
        }
    }
}
