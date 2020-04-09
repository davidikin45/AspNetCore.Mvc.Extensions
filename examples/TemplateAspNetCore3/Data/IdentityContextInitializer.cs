using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.StartupTasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data.Initializers;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data
{
    public class IdentityContextInitializer : IDbStartupTask
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IdentityContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public int Order => 0;

        public IdentityContextInitializer(IdentityContext context, IPasswordHasher<User> passwordHasher, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_hostingEnvironment.IsStaging() || _hostingEnvironment.IsProduction())
            {
                var migrationInitializer = new IdentityContextInitializerMigrate(_passwordHasher);
                await migrationInitializer.InitializeAsync(_context);
            }
            else if (_hostingEnvironment.IsIntegration())
            {
                var migrationInitializer = new IdentityContextInitializerDropMigrate(_passwordHasher);
                await migrationInitializer.InitializeAsync(_context);
            }
            else
            {
                var migrationInitializer = new IdentityContextInitializerDropCreate(_passwordHasher);
                await migrationInitializer.InitializeAsync(_context);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
