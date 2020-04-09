using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions
{
    public class Config
    {
        private static readonly Dictionary<string, string> InMemoryDefaults = new Dictionary<string, string> {
            {
                WebHostDefaults.EnvironmentKey, "Development"
            },
            {
                WebHostDefaults.WebRootKey, "wwwroot"
            }
        };

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.2
        //Host configuration values 
        //applicationName - Defaults to  The name of the assembly containing the app's entry point. This configuration setting specifies the value that will be returned from IHostingEnvironment.ApplicationName
        //captureStartupErrors - Defaults to false. When false, errors during startup result in the host exiting. When true, the host will capture any exceptions from the Startup class and attempt to start the server. It will display an error page (generic, or detailed, based on the Detailed Errors setting, below) for every request.
        //contentRoot - This setting determines where ASP.NET Core will begin searching for content files, such as MVC Views. Also used as the base path for the Web Root setting. Set using the UseContentRoot method. Path must exist, or host will fail to start.
        //detailedErrors - Defaults to false. When true (or when Environment is set to “Development”), the app will display details of startup exceptions, instead of just a generic error page. Set using UseSetting.
        //environment - Defaults to “Production”. May be set to any value. Framework-defined values include “Development”, “Staging”, and “Production”. Values are not case sensitive.
        //preventHostingStartup - Default false. Prevents the automatic loading of hosting startup assemblies, including hosting startup assemblies configured by the app's assembly.
        //preferHostingUrls - Default true. Indicates whether the host should listen on the URLs configured with the WebHostBuilder instead of those configured with the IServer implementation.
        //urls - Set to a semicolon (;) separated list of URL prefixes to which the server should respond. For example, “http://localhost:123”. The domain/host name can be replaced with “*” to indicate the server should listen to requests on any IP address or host using the specified port and protocol (for example, “http://:5000” or “https://:5001”). The protocol (“http://” or “https://”) must be included with each URL. The prefixes are interpreted by the configured server; supported formats will vary between servers.
        //startupAssembly - Determines the assembly to search for the Startup class.
        //hostingStartupAssemblies - Determines the assembly to search for the Startup class. Set using the UseStartup method. May instead reference specific type using WebHostBuilder.UseStartup<StartupType>
        //hostingStartupExcludeAssemblies - A semicolon-delimited string of hosting startup assemblies to exclude on startup.
        //https_port - Set the HTTPS redirect port.
        //shutdownTimeoutSeconds - default  5
        //webroot - if not specified the default is (Content Root Path)\wwwroot, if it exists. If this path doesn’t exist, then a no-op file provider is used. Set using UseWebRoot.

        /// <summary>
        ///  Config.Build(args, Directory.GetCurrentDirectory(), typeof(TStartup).Assembly.GetName().Name);
        /// </summary>
        public static IConfigurationRoot Build(string[] args, string contentRoot, string assemblyName)
        {
            var configEnvironmentBuilder = new ConfigurationBuilder()
                   .AddInMemoryCollection(InMemoryDefaults)
                   .AddEnvironmentVariables(prefix: "ASPNETCORE_");

            if (args != null)
            {
                configEnvironmentBuilder.AddCommandLine(args);
            }
            var configEnvironment = configEnvironmentBuilder.Build();

            var appSettingsFileName = "appsettings.json";
            var appSettingsEnvironmentFilename = "appsettings." + (configEnvironment[WebHostDefaults.EnvironmentKey] ?? "Production") + ".json";

            //https://andrewlock.net/introducing-the-microsoft-featuremanagement-library-adding-feature-flags-to-an-asp-net-core-app-part-1/
            var featuresFileName = "features.json";
            var featuresEnvironmentFilename = "features." + (configEnvironment[WebHostDefaults.EnvironmentKey] ?? "Production") + ".json";

            Console.WriteLine($"Loading Settings:" + Environment.NewLine +
                               $"{contentRoot}\\{appSettingsFileName}" + Environment.NewLine +
                               $"{contentRoot}\\{appSettingsEnvironmentFilename}" + Environment.NewLine +
                               $"{contentRoot}\\{featuresFileName}" + Environment.NewLine +
                               $"{contentRoot}\\{featuresEnvironmentFilename}");

            var settingSuffix = assemblyName.ToUpperInvariant().Replace(".", "_");
            var settingName = $"TEST_CONTENTROOT_{settingSuffix}";
            Dictionary<string, string> InMemoryDefaultsContentRoot = new Dictionary<string, string> { {
                settingName, contentRoot
            } };

            var config = new ConfigurationBuilder()
           .AddInMemoryCollection(InMemoryDefaults)
           .AddInMemoryCollection(InMemoryDefaultsContentRoot)
           .SetBasePath(contentRoot)

           .AddJsonFile(appSettingsFileName, optional: false, reloadOnChange: true)
           .AddJsonFile(appSettingsEnvironmentFilename, optional: true, reloadOnChange: true)

           .AddJsonFile(featuresFileName, optional: true, reloadOnChange: true)
           .AddJsonFile(featuresEnvironmentFilename, optional: true, reloadOnChange: true);

            if (configEnvironment[WebHostDefaults.EnvironmentKey].ToLower() == "development")
            {
                var appAssembly = Assembly.Load(new AssemblyName(assemblyName));
                if (appAssembly != null)
                {
                    config.AddUserSecrets(appAssembly, optional: true);
                }
            }

            config.AddEnvironmentVariables(prefix: "ASPNETCORE_");

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            return config.Build();
        }

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.2
        //Host configuration values 
        //applicationName - Defaults to  The name of the assembly containing the app's entry point. This configuration setting specifies the value that will be returned from IHostingEnvironment.ApplicationName
        //captureStartupErrors - Defaults to false. When false, errors during startup result in the host exiting. When true, the host will capture any exceptions from the Startup class and attempt to start the server. It will display an error page (generic, or detailed, based on the Detailed Errors setting, below) for every request.
        //contentRoot - This setting determines where ASP.NET Core will begin searching for content files, such as MVC Views. Also used as the base path for the Web Root setting. Set using the UseContentRoot method. Path must exist, or host will fail to start.
        //detailedErrors - Defaults to false. When true (or when Environment is set to “Development”), the app will display details of startup exceptions, instead of just a generic error page. Set using UseSetting.
        //environment - Defaults to “Production”. May be set to any value. Framework-defined values include “Development”, “Staging”, and “Production”. Values are not case sensitive.
        //preventHostingStartup - Default false. Prevents the automatic loading of hosting startup assemblies, including hosting startup assemblies configured by the app's assembly.
        //preferHostingUrls - Default true. Indicates whether the host should listen on the URLs configured with the WebHostBuilder instead of those configured with the IServer implementation.
        //urls - Set to a semicolon (;) separated list of URL prefixes to which the server should respond. For example, “http://localhost:123”. The domain/host name can be replaced with “*” to indicate the server should listen to requests on any IP address or host using the specified port and protocol (for example, “http://:5000” or “https://:5001”). The protocol (“http://” or “https://”) must be included with each URL. The prefixes are interpreted by the configured server; supported formats will vary between servers.
        //startupAssembly - Determines the assembly to search for the Startup class.
        //hostingStartupAssemblies - Determines the assembly to search for the Startup class. Set using the UseStartup method. May instead reference specific type using WebHostBuilder.UseStartup<StartupType>
        //hostingStartupExcludeAssemblies - A semicolon-delimited string of hosting startup assemblies to exclude on startup.
        //https_port - Set the HTTPS redirect port.
        //shutdownTimeoutSeconds - default  5
        //webroot - if not specified the default is (Content Root Path)\wwwroot, if it exists. If this path doesn’t exist, then a no-op file provider is used. Set using UseWebRoot.
        public static IConfigurationRoot BuildHostingConfig(string[] args, string contentRoot, string assemblyName)
        {
            var configEnvironmentBuilder = new ConfigurationBuilder()
                   .AddInMemoryCollection(InMemoryDefaults)
                   .AddEnvironmentVariables(prefix: "ASPNETCORE_");

            if (args != null)
            {
                configEnvironmentBuilder.AddCommandLine(args);
            }
            var configEnvironment = configEnvironmentBuilder.Build();

            var appSettingsFileName = "hosting.json";
            var appSettingsEnvironmentFilename = "hosting." + (configEnvironment[WebHostDefaults.EnvironmentKey] ?? "Production") + ".json";

            var featuresFileName = "features.json";
            var featuresEnvironmentFilename = "features." + (configEnvironment[WebHostDefaults.EnvironmentKey] ?? "Production") + ".json";

            Console.WriteLine($"Loading Settings:" + Environment.NewLine +
                               $"{contentRoot}\\{appSettingsFileName}" + Environment.NewLine +
                               $"{contentRoot}\\{appSettingsEnvironmentFilename}" + Environment.NewLine +
                               $"{contentRoot}\\{featuresFileName}" + Environment.NewLine +
                               $"{contentRoot}\\{featuresEnvironmentFilename}");

            var config = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryDefaults)
            .SetBasePath(contentRoot)
            .AddJsonFile(appSettingsFileName, optional: false, reloadOnChange: true)
            .AddJsonFile(appSettingsEnvironmentFilename, optional: true, reloadOnChange: true)

            .AddJsonFile(featuresFileName, optional: true, reloadOnChange: true)
            .AddJsonFile(featuresEnvironmentFilename, optional: true, reloadOnChange: true);

            if (configEnvironment[WebHostDefaults.EnvironmentKey].ToLower() == "development")
            {
                var appAssembly = Assembly.Load(new AssemblyName(assemblyName));
                if (appAssembly != null)
                {
                    config.AddUserSecrets(appAssembly, optional: true);
                }
            }

            config.AddEnvironmentVariables(prefix: "ASPNETCORE_");

            if (args != null)
            {
                config.AddCommandLine(args);
            }

            return config.Build();
        }
    }
}
