using AspNetCore.Mvc.Extensions.Logging;
using Autofac.AspNetCore.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions
{
    public abstract class ProgramBase<TStartup> where TStartup : class
    {
        public static IConfiguration Configuration;

        public static IConfiguration BuildWebHostConfiguration(string environment, string contentRoot)
        {
            return Config.Build(new string[] { $"environment={environment}" }, contentRoot, typeof(TStartup).Assembly.GetName().Name);
        }

        public static async Task<int> RunApp(string[] args)
        {

            Configuration = Config.Build(args, Directory.GetCurrentDirectory(), typeof(TStartup).Assembly.GetName().Name);

            AssemblyHelper.EntryAssembly = typeof(TStartup).Assembly;

            LoggingInit.Init(Configuration);

            try
            {
                Log.Information("Getting the motors running...");

                var host = CreateHostBuilder(args).Build();

                //.NET Core 2.2 
                //var host = CreateWebHostBuilder(args).Build();

                //https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-2/
                //Even though the tasks run after the IConfiguration and DI container configuration has completed, they run before the IStartupFilters have run and the middleware pipeline has been configured.
                //await host.InitAsync();

                //AppStartup.Configure will be called here
                await host.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        //.NET Core 3.0
        public static IHostBuilder CreateHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args)
           //https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/?fbclid=IwAR2SZoIsGjtTfwCd5bEG9n0mpnbo-3ERVCYZk6snBDnbIHwKC5dYbIoj_vY
           .UseSerilog()
           .UseAutofacMultitenant((context, options) => {
                options.ValidateOnBuild = false;
                options.MapDefaultTenantToAllRootDomains();
                options.AddTenantsFromConfig(context.Configuration);
                options.ConfigureTenants(builder => {
                    builder.MapToTenantIdSubDomain();
                });
            })

            //builder.UseDefaultServiceProvider((context, options) => {
            //    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            //    options.ValidateOnBuild = context.HostingEnvironment.IsDevelopment();
            //})

            //https://www.hanselman.com/blog/dotnetNewWorkerWindowsServicesOrLinuxSystemdServicesInNETCore.aspx?fbclid=IwAR0ldO7MnDkgjK-dajY-dSEGt8aHvS70EC8Rvbl8RkPMXC1cJLIdbthQIME
            .UseWindowsService()
            .ConfigureHostConfiguration(config =>
            {
                // Uses DOTNET_ environment variables and command line args
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                // JSON files, User secrets, environment variables and command line arguments
            })
            .ConfigureServices((hostContext, services) =>
            {

                //services.AddHostedService<ServiceA>();
            })
            //Only required if the service responds to requests.
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureWebHost(ConfigureWebHost);
            });

        //.NET Core 2.2 
        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //    .UseAutofacMultitenant((context,options) => {
        //        options.MapDefaultTenantToAllRootDomains();
        //        options.AddTenantsFromConfig(context.Configuration);
        //        options.ConfigureTenants(builder => {
        //            builder.MapToTenantIdSubDomain();
        //        });
        //    })

        //    //builder.UseDefaultServiceProvider((context, options) => {
        //    //    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
        //    //    options.ValidateOnBuild = context.HostingEnvironment.IsDevelopment();
        //    //})

        //    .ConfigureAppConfiguration((context, config) =>
        //        {
                 
        //            // JSON files, User secrets, environment variables and command line arguments
        //        })
        //    .ConfigureServices((hostContext, services) =>
        //    {
        //        //services.AddHostedService<ServiceA>();
        //    })
        //    .ConfigureWebHost(ConfigureWebHost);

        private static void ConfigureWebHost(IWebHostBuilder webBuilder)
        {
            webBuilder
            // These two settings allow an error page to be shown rather than throwing exception on startup
            // Need to be careful putting code after IWebHostBuilder.Build()
            .CaptureStartupErrors(true)
            .UseShutdownTimeout(TimeSpan.FromSeconds(20))
            //.UseSetting("detailedErrors", "true") // Better to put this in appsettings
            .ConfigureKestrel((context, options) =>
            {
                if (context.HostingEnvironment.IsDevelopment() || context.HostingEnvironment.IsIntegration())
                {
                    options.ListenAnyIP(5000);
                    options.ListenAnyIP(5001, listenOptions =>
                    {
                        //listenOptions.UseHttps(new X509Certificate2("certificates\\localhost.private.pfx", "password"));
                        listenOptions.UseHttps();
                    });
                }

                options.AllowSynchronousIO = true;
                options.AddServerHeader = false;
            }
            ).
            UseConfiguration(Configuration) ////IWebHostBuilder configuration is added to the app's configuration, but the converse isn't true. ConfigureAppConfiguration doesn't affect the IWebHostBuilder configuration.
            .UseIISIntegration()
            .UseAzureKeyVault()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {

            })
            .ConfigureServices(services =>
            {
                services.AddHttpContextAccessor();
            })
            .UseStartupTasks(false)
            //https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.0&tabs=visual-studio
            //When the app is published, the companion assets from all referenced projects and packages are copied into the wwwroot folder of the published app under _content/{LIBRARY NAME}/.
            //When running the consuming app from build output (dotnet run), static web assets are enabled by default in the Development environment. To support assets in other environments when running from build output, call UseStaticWebAssets on the host builder in Program.cs:
            //alling UseStaticWebAssets isn't required when running an app from published output (dotnet publish).
            .UseStaticWebAssets() //wwwroot from class library https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.0&tabs=visual-studio

            //.NET Core 2.2 
            //https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/?fbclid=IwAR2SZoIsGjtTfwCd5bEG9n0mpnbo-3ERVCYZk6snBDnbIHwKC5dYbIoj_vY
            //.UseSerilog()

            .UseStartup<TStartup>();
        }

        //WebHostBuilder - https://github.com/aspnet/Hosting/blob/3483a3250535da6f291326f3f5f1e3f66ca09901/src/Microsoft.AspNetCore.Hosting/WebHostBuilder.cs
        //WebHost.CreateDefaultBuilder(args) - https://github.com/aspnet/MetaPackages/blob/release/2.1/src/Microsoft.AspNetCore/WebHost.cs
        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.1

        // Only used by EF Core Tooling if IDesignTimeDbContextFactory is not implemented
        // Generally its not good practice to DB in the MVC Project so best to use IDesignTimeDbContextFactory
        //https://wildermuth.com/2017/07/06/Program-cs-in-ASP-NET-Core-2-0
        // public static IWebHost BuildWebHost(string[] args)
        //{
        // Configuration = BuildWebHostConfiguration(args, Directory.GetCurrentDirectory());
        //return CreateWebHostBuilder(args).Build();
        //}
    }
}
