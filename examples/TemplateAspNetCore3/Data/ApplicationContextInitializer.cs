using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.StartupTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data.Initializers;

namespace TemplateAspNetCore3.Data
{
    public class ApplicationContextInitializer : IDbStartupTask
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly AppContext _context;

        public int Order => 0;

        public ApplicationContextInitializer(AppContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_hostingEnvironment.IsStaging() || _hostingEnvironment.IsProduction())
            {
                var dbInitializer = new AppContextInitializerMigrate();
                await dbInitializer.InitializeAsync(_context);
            }
            else if (_hostingEnvironment.IsIntegration())
            {
                var dbInitializer = new AppContextInitializerDropMigrate();
                await dbInitializer.InitializeAsync(_context);
            }
            else
            {
                var dbInitializer = new AppContextInitializerDropMigrate();
                await dbInitializer.InitializeAsync(_context);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
