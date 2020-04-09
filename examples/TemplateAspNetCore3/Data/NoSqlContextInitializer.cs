using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.StartupTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data.Initializers;

namespace TemplateAspNetCore3.Data
{
    public class NoSqlContextInitializer : IDbStartupTask
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly NoSqlContext _context;

        public int Order => 0;

        public NoSqlContextInitializer(NoSqlContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_hostingEnvironment.IsStaging() || _hostingEnvironment.IsProduction())
            {
                var dbInitializer = new NoSqlContextInitializerMigrate();
                await dbInitializer.InitializeAsync(_context);
            }
            else if (_hostingEnvironment.IsIntegration())
            {
                var dbInitializer = new NoSqlContextInitializerDropCreate();
                await dbInitializer.InitializeAsync(_context);
            }
            else
            {
                var dbInitializer = new NoSqlContextInitializerDropCreate();
                await dbInitializer.InitializeAsync(_context);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
