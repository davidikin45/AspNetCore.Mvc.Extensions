using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.ApiClient;
using AspNetCore.Mvc.Extensions.Attributes.Validation;
using AspNetCore.Mvc.Extensions.Authentication;
using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Authorization.PolicyProviders;
using AspNetCore.Mvc.Extensions.Authorization.Requirements;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Conventions.Display;
using AspNetCore.Mvc.Extensions.Cqrs;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Features;
using AspNetCore.Mvc.Extensions.HealthChecks;
using AspNetCore.Mvc.Extensions.IntegrationEvents;
using AspNetCore.Mvc.Extensions.Json;
using AspNetCore.Mvc.Extensions.Logging.Serilog;
using AspNetCore.Mvc.Extensions.Middleware;
using AspNetCore.Mvc.Extensions.Notifications;
using AspNetCore.Mvc.Extensions.Reflection;
using AspNetCore.Mvc.Extensions.Routing;
using AspNetCore.Mvc.Extensions.Routing.Constraints;
using AspNetCore.Mvc.Extensions.Security;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.SignalR;
using AspNetCore.Mvc.Extensions.Validation.Errors;
using AspNetCore.Mvc.MvcAsApi.ActionResults;
using AspNetCore.Mvc.MvcAsApi.Conventions;
using AspNetCore.Mvc.MvcAsApi.Extensions;
using AspNetCore.Mvc.MvcAsApi.Middleware;
using AspNetCore.Mvc.MvcAsApi.ModelBinding;
using AspNetCore.Mvc.SelectList;
using AspNetCore.Mvc.UrlLocalization;
using AspNetCore.Mvc.UrlLocalization.AmbientRouteData;
using AspNetCore.Specification;
using AspNetCore.Specification.OrderByMapping;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.AspNetCore.Extensions;
using Ben.Diagnostics;
using FluentValidation.AspNetCore;
using GraphQL;
using GraphQL.Server;
using Hangfire.AspNetCore.Extensions;
using Hangfire.AspNetCore.Multitenant;
using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Westwind.AspNetCore.LiveReload;

namespace AspNetCore.Mvc.Extensions
{
    //https://andrewlock.net/ihostingenvironment-vs-ihost-environment-obsolete-types-in-net-core-3/
    //Microsoft.Extensions.Hosting.IHostingEnvironment > Microsoft.AspNetCore.Hosting.IHostEnvironment
    //Microsoft.AspNetCore.Hosting.IHostingEnvironment > Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    //Microsoft.Extensions.Hosting.IApplicationLifetime > Microsoft.Extensions.Hosting.IHostApplicationLifetime
    //Microsoft.AspNetCore.Hosting.IApplicationLifetime > Microsoft.Extensions.Hosting.IHostApplicationLifetime

    public abstract class AppStartup
    {
        public class AppStartupOptions
        { 
            public bool BlazorServerApp { get; set; } = false;
            public bool BlazorWASMApp { get; set; } = false;
            public bool AngularApp { get; set; } = false;
            public bool ReactApp { get; set; } = false;

            public bool ScanAssembliesForDbStartupTasks { get; set; } = false;
            public bool ScanAssembliesForStartupTasks { get; set; } = false;
            public bool AddServicesByConvention { get; set; } = true;
            public bool DetectBlocking { get; set; } = false;
            public bool TreatNullValueAsNoContent { get; set; } = true;

        }

        //https://github.com/aspnet/Extensions/issues/1096
        //.NET Core 3.0. IHostingEnvironment and IConfiguration only
        public AppStartup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment, Action<AppStartupOptions> config = null)
        {
            AssemblyHelper.EntryAssembly = this.GetType().Assembly;
    
            Options = new AppStartupOptions();
            if (config != null)
                config(Options);

            Logger = new SerilogLoggerFactory().CreateLogger("Startup");
            //Logger = loggerFactory.CreateLogger("Startup");
  
            Configuration = configuration;

            AppSettings = GetSettings<AppSettings>("AppSettings");

            HostingEnvironment = hostingEnvironment;

            //http://blog.hostforlife.eu/variables-and-configuration-in-asp-net-core-apps/
            //http://www.hishambinateya.com/goodbye-platform-abstractions
            //var workingDirectory = Directory.GetCurrentDirectory();
            WorkingDirectory = hostingEnvironment.ContentRootPath;

            //AppDomain.CurrentDomain.BaseDirectory
            //Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
            BinPath = AppContext.BaseDirectory;
            Logger.LogInformation($"Bin Folder: {BinPath}");

            PluginsPath = Path.Combine(BinPath, PluginsFolder);
            Logger.LogInformation($"Plugins Folder: {PluginsPath}");
            if (!Directory.Exists(PluginsPath)) Directory.CreateDirectory(PluginsPath);

            //Logs should always be relative to the Working Directory. Thats how serilog works.
            LogsPath = Path.Combine(WorkingDirectory, LogsFolder);
            Logger.LogInformation($"Logs Folder: {LogsPath}");
            if (!Directory.Exists(LogsPath)) Directory.CreateDirectory(LogsPath);

            //Data should generally be relative to the Working Directory.
            DBPath = Path.Combine(WorkingDirectory, DBFolder);
            Logger.LogInformation($"DB Folder: {DBPath}");
            if (!Directory.Exists(DBPath)) Directory.CreateDirectory(DBPath);

            BinDBPath = Path.Combine(BinPath, DBFolder);
            Logger.LogInformation($"Bin DB Folder: {BinDBPath}");
            if (!Directory.Exists(BinDBPath)) Directory.CreateDirectory(BinDBPath);

            foreach (var publicUploadFolder in AppSettings.PublicUploadFolders.Split(','))
            {
                var path = HostingEnvironment.WebRootPath + publicUploadFolder;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            Logger.LogInformation($"Content Root Path (Working Directory): {hostingEnvironment.ContentRootPath}");
            Logger.LogInformation($"Web Root Path: {hostingEnvironment.WebRootPath}");

            AssemblyName = this.GetType().Assembly.GetName().Name;
            AppAssemblyPrefix = configuration.GetValue<string>("AppSettings:AssemblyPrefix");

            AssemblyBoolFilter = (a => a.FullName.Contains(AppAssemblyPrefix) || a.FullName.Contains(CommonAssemblyPrefix));
            AssemblyStringFilter = (s => (new FileInfo(s)).Name.Contains(AppAssemblyPrefix) || (new FileInfo(s)).Name.Contains(CommonAssemblyPrefix));

            //https://dotnetstories.com/blog/Dynamically-pre-load-assemblies-in-a-ASPNET-Core-or-any-C-project-en-7155735300
            //https://weblog.west-wind.com/posts/2012/Nov/03/Back-to-Basics-When-does-a-NET-Assembly-Dependency-get-loaded
            //Assemblies only get loaded into loadAppDomain.CurrentDomain.GetAssemblies() after it's actually referenced.
            //Can never unload an assembly.
            //Dependent Assembly References are not pre-loaded when an application starts (by default).
            //Dependent Assemblies that are not referenced by executing code are never loaded.
            //Dependent Assemblies are just in time loaded when first referenced in code.
            //Once Assemblies are loaded they can never be unloaded, unless the AppDomain that host them is unloaded.

            //For Plugin Class Libraries in Debug/Release mode.
            //<PropertyGroup>
            //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
            //</PropertyGroup>

            //Load plugins into current AppDomain
            //AssemblyLoader.LoadAssembliesFromPath(PluginsPath);

            //Load Assemblies into current AppDomain - This allows for assembly scanning.
            //AssemblyLoader.LoadApplicationDependencies();

            //ApplicationParts = AssemblyLoader.GetLoadedAssemblies(AssemblyBoolFilter);

            //https://cardano.github.io/blog/2017/04/25/thinking-of-using-GetCallingAssembly
        }

        public Microsoft.Extensions.Logging.ILogger Logger { get; }

        public AppStartupOptions Options { get; }

        //public IWebHostEnvironment HostingEnvironment { get; }

        //old
        public Microsoft.AspNetCore.Hosting.IWebHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        public AppSettings AppSettings { get; }

        public string BinPath { get; }
        public string PluginsPath { get; }
        public string WorkingDirectory { get; }

        public string LogsPath { get; }
        public string DBPath { get; }
        public string BinDBPath { get; }

        public string AssemblyName { get; }
        public string AppAssemblyPrefix { get; }
        public string CommonAssemblyPrefix { get; } = "AspNetCore.Mvc.Extensions";

        public string PluginsFolder { get; } = @"plugins\";
        public string LogsFolder { get; } = @"logs\";
        public string DBFolder { get; } = @"db\";
        public string AssetsFolder { get; } = "/files";

        public Func<Assembly, Boolean> AssemblyBoolFilter { get; }
        public Func<string, Boolean> AssemblyStringFilter { get; }

        public List<Assembly> ApplicationParts { get; private set; }

        #region 1. Configure Services
        //When the app is running in the Development environment, the default service provider performs checks to verify that:
        //Scoped services aren't directly or indirectly resolved from the root service provider.
        //Scoped services aren't directly or indirectly injected into singletons.
        // Add services to the collection. Don't build or return
        // any IServiceProvider or the ConfigureContainer method
        // won't get called.
        //Disposal: https://stackoverflow.com/questions/40844151/when-are-net-core-dependency-injected-instances-disposed
        //If there are multiple registrations for the same service type the last registered type wins.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            //App Dependencies
            var appDependencies = DependencyContext.Default;

            //https://github.com/dotnet/samples/tree/master/core/extensions/AppWithPlugin
            //https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
            //Load plugins into current AppDomain
            //AssemblyLoader.LoadPluginAssembliesFromPath(PluginsPath);
            //https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support

            //.NET Core 3.0
            services.LoadPluginAssembliesFromPath(PluginsPath);

            //Load Assemblies into current AppDomain - This allows for assembly scanning.
            //AssemblyLoader.LoadApplicationDependencies();
            services.LoadApplicationDependencies();

            //Get Assembly Parts
            ApplicationParts = AssemblyLoader.GetLoadedAssemblies(AssemblyBoolFilter);

            //Web Farm
            //https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-2.2
            //https://stackoverflow.com/questions/38795103/encrypt-string-in-net-core/38797581

            ConfigureSettingsServices(services);

            ConfigureStartupTasks(services);
            ConfigureDatabaseServices(services);
            ConfigureApplicationServices(services);
            ConfigureAuthenticationServices(services);

            //This will override defaultauthentication
            ConfigureIdentityServices(services);

            ConfigureContextServices(services);
            ConfigureFeatureServices(services);
            ConfigureAuthorizationServices(services);
            ConfigureSecurityServices(services);
            ConfigureEmailServices(services);
            ConfigureCachingServices(services);
            ConfigureResponseCompressionServices(services);
            ConfigureLocalizationServices(services);
            ConfigureMvcServices(services);
            ConfigureBlazorServices(services);
            ConfigureValidationServices(services);

            ConfigureEventServices(services);
            ConfigureSignalRServices(services);
            ConfigureApiServices(services);
            ConfigureHealthCheckServices(services);
            ConfigureHttpClients(services);

            //.NET Core 3.0
            ConfiguregRPCClients(services);

            ConfigureProfilingServices(services);
            ConfigureHostedServices(services);
            ConfigureLiveReloadServices(services);

            AddHangfireJobServices(services);

            //Generally assemblies are only loaded as they are loaded.
            if (Options.AddServicesByConvention)
            {
                services.AddServicesByConvention(options =>
                {
                    options.LoadApplicationDependencies = false;
                    options.Predicate = AssemblyBoolFilter;
                });
            }

            services.AddOrderByMapper(ConfigureOrderByMapper);
            services.AddAutoMapperInterfaces(options =>
            {
                options.LoadApplicationDependencies = false;
                options.Predicate = AssemblyBoolFilter;
            });
        }

        public abstract void ConfigureOrderByMapper(OrderByMapperOptions options);

        #region Settings
        //Dictionary must use string key!
        //https://andrewlock.net/how-to-use-the-ioptions-pattern-for-configuration-in-asp-net-core-rc2/
        public virtual void ConfigureSettingsServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Settings");

            //Using ASP.NET Core 2.0 will automatically add the IConfiguration instance of your application in the dependency injection container
            services.AddSingleton(Configuration);

            //Validate Settings on Startup.
            services.UseStartupSettingsValidation();

            services.ConfigureSettings<AppSettings>(Configuration.GetSection("AppSettings"));
            services.ConfigureSettings<LocalizationSettings>(Configuration.GetSection("LocalizationSettings"));

            services.ConfigureSettings<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.PostConfigure<ConnectionStrings>(options =>
            {
                ManipulateConnectionStrings(options);
            });

            services.ConfigureSettings<ServerSettings>(Configuration.GetSection("ServerSettings"));
            services.ConfigureSettings<ApiClientSettings>(Configuration.GetSection("ApiClientSettings"));

            services.ConfigureSettings<TokenSettings>(Configuration.GetSection("TokenSettings"));
            services.PostConfigure<TokenSettings>(options =>
            {
                ManipulateTokenSettings(options);
            });

            services.ConfigureSettings<CORSSettings>(Configuration.GetSection("CORSSettings"));
            services.ConfigureSettings<PasswordSettings>(Configuration.GetSection("PasswordSettings"));
            services.ConfigureSettings<UserSettings>(Configuration.GetSection("UserSettings"));
            services.ConfigureSettings<AuthenticationSettings>(Configuration.GetSection("AuthenticationSettings"));
            services.ConfigureSettings<AuthorizationSettings>(Configuration.GetSection("AuthorizationSettings"));
            services.ConfigureSettings<CacheSettings>(Configuration.GetSection("CacheSettings"));

            services.ConfigureSettings<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.PostConfigure<EmailSettings>(options =>
            {
                ManipulateEmailSettings(options);
            });

            services.ConfigureSettings<EmailTemplates>(Configuration.GetSection("EmailTemplates"));
            services.PostConfigure<EmailTemplates>(options =>
            {
                ManipluateEmailTemplateSettings(options);
            });

            services.ConfigureSettings<ElasticSettings>(Configuration.GetSection("ElasticSettings"));
            services.ConfigureSettings<SwitchSettings>(Configuration.GetSection("SwitchSettings"));
            services.ConfigureSettings<AssemblyProviderOptions>(options =>
            {
                options.BinPath = BinPath;
                options.AssemblyFilter = AssemblyStringFilter;
            });
        }

        //By default SQL and SQLite databases are created within the Working Directory = Directory.GetCurrentDirectory()
        //%BINDATA% creates the SQL and SQLite databases within the Bin Directory = AppContext.BaseDirectory
        private ConnectionStrings ManipulateConnectionStrings(ConnectionStrings options)
        {
            var keys = options.Keys.ToList();

            foreach (var key in keys)
            {
                if (options[key].Contains("%DB%") || options[key].Contains("%DATA%"))
                {
                    options[key] = options[key].Replace("%DB%", DBPath).Replace("%DATA%", DBPath);
                }

                if (options[key].Contains("%BINDB%") || options[key].Contains("%BINDATA%"))
                {
                    options[key] = options[key].Replace("%BINDB%", BinDBPath).Replace("%BINDATA%", BinDBPath);
                }

                if (options[key].Contains("%BIN%"))
                {
                    options[key] = options[key].Replace("%BIN%", BinPath);
                }
            }

            return options;
        }

        private TokenSettings ManipulateTokenSettings(TokenSettings options)
        {
            if (!string.IsNullOrEmpty(options.PrivateKeyPath))
            {
                options.PrivateKeyPath = HostingEnvironment.MapContentPath(options.PrivateKeyPath);
            }

            if (!string.IsNullOrEmpty(options.PublicKeyPath))
            {
                options.PublicKeyPath = HostingEnvironment.MapContentPath(options.PublicKeyPath);
            }

            if (!string.IsNullOrEmpty(options.PrivateCertificatePath))
            {
                options.PrivateCertificatePath = HostingEnvironment.MapContentPath(options.PrivateCertificatePath);
            }

            if (!string.IsNullOrEmpty(options.PublicCertificatePath))
            {
                options.PublicCertificatePath = HostingEnvironment.MapContentPath(options.PublicCertificatePath);
            }

            return options;
        }

        private void ManipluateEmailTemplateSettings(EmailTemplates options)
        {
            if (!string.IsNullOrEmpty(options.Welcome))
            {
                options.Welcome = HostingEnvironment.MapContentPath(options.Welcome);
            }

            if (!string.IsNullOrEmpty(options.ResetPassword))
            {
                options.ResetPassword = HostingEnvironment.MapContentPath(options.ResetPassword);
            }
        }

        private void ManipulateEmailSettings(EmailSettings options)
        {
            if (!options.FileSystemFolder.Contains(@":\"))
            {
                options.FileSystemFolder = Path.Combine(WorkingDirectory, options.FileSystemFolder);
            }
        }

        public TSettings GetSettings<TSettings>(string sectionKey) where TSettings : class
        {
            var settingsSection = Configuration.GetSection(sectionKey);
            return settingsSection.Get<TSettings>();
        }

        #endregion

        #region Startup Tasks Before Accepting Requests
        public virtual void ConfigureStartupTasks(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Startup Tasks");

            AddDbStartupTasks(services);

            if (Options.ScanAssembliesForDbStartupTasks)
            {
                services.AddDbStartupTasks(options =>
                {
                    options.LoadApplicationDependencies = false;
                    options.Predicate = AssemblyBoolFilter;
                });
            }

            AddStartupTasks(services);

            if (Options.ScanAssembliesForStartupTasks)
            {
                services.AddStartupTasks(options =>
                {
                    options.LoadApplicationDependencies = false;
                    options.Predicate = AssemblyBoolFilter;
                });
            }
        }
        #endregion

        #region Database
        public virtual void ConfigureDatabaseServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Databases");

            var connectionStrings = ManipulateConnectionStrings(GetSettings<ConnectionStrings>("ConnectionStrings"));

            var identityConnectionString = connectionStrings.ContainsKey("IdentityConnection") ? connectionStrings["IdentityConnection"] : null;
            var defaultConnectionString = connectionStrings.ContainsKey("DefaultConnection") ? connectionStrings["DefaultConnection"] : null;
            //Configuration.GetSection("ConnectionStrings").GetChildren().Any(x => x.Key == "HangfireConnection") ? Configuration.GetConnectionString("HangfireConnection").Replace("%BIN%", BinPath).Replace("%DATA%", DataPath) : null;
            var hangfireConnectionString = connectionStrings.ContainsKey("HangfireConnection") ? connectionStrings["HangfireConnection"] : null;

            AddDatabases(services, connectionStrings, identityConnectionString, hangfireConnectionString, defaultConnectionString);
            AddRepositories(services);
            AddUnitOfWorks(services);

            // Add the processing server as IHostedService
            services.AddHangfireServer("web-background", hangfireConnectionString, options => options.PrepareSchemaIfNecessary = false);

            //https://github.com/HangfireIO/Hangfire/blob/a604028fa8a75ea1a122f58464e0823a076e6431/src/Hangfire.AspNetCore/HangfireServiceCollectionExtensions.cs
            // Add the processing server as IHostedService
            //services.AddHangfireServer();
        }
        #endregion

        #region Application Servicea
        public virtual void ConfigureApplicationServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Application Services");

            AddApplicationServices(services);
        }
        #endregion

        #region Domain Servicea
        public virtual void ConfigureDomainServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Domain Services");

            AddDomainServices(services);
        }
        #endregion

        #region Features
        public virtual void ConfigureFeatureServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Features");

            //https://andrewlock.net/introducing-the-microsoft-featuremanagement-library-adding-feature-flags-to-an-asp-net-core-app-part-1/
            //[FeatureGate("feature")] - Generates 404.
            //@inject  Microsoft.FeatureManagement.IFeatureManagerSnapshot _featureManager;   @if (_featureManager.IsEnabled(FeatureFlags.Beta)); <feature name="@FeatureFlags.Beta">
            //services.AddSession();
            //services.AddHttpContextAccessor();
            //services.AddTransient<ISessionManager, SessionSessionManager>();

            services.AddFeatureManagement()
            .UseDisabledFeaturesHandler(new RedirectDisabledFeatureToForbiddenHandler())
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<TimeWindowFilter>();
        }
        #endregion

        #region Data Protection
        public virtual void ConfigureDataProtectionServices(IServiceCollection services)
        {
            //https://jakeydocs.readthedocs.io/en/latest/security/data-protection/implementation/key-encryption-at-rest.html
            //https://edi.wang/post/2019/1/15/caveats-in-aspnet-core-data-protection -- Encryption
            //https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-2.2
            //keys are persisted to the %LOCALAPPDATA%\ASP.NET\DataProtection-Keys folder. 
            //C:\Users\david\AppData\Local\ASP.NET\DataProtection-Keys

            //https://scripts.cmbuckley.co.uk/cookies.php - Subdomains are ok as long as there are no XSS holes.
            //IDataProtectionProvider for short-lived Encryption/Decryption
            //_antiForgery.GetAndStoreTokens(HttpContext) will only add cookie if it doesn't already exist. Will generate a new request token each time.
            //Antiforgery is essential meaning cookie policy doesn't matter.
            //The same unencrypted value is stored in cookie .AspNetCore.Antiforgery.PMN-jCSZaPU AND __RequestVerificationToken but they use different salts.
            //unencrypted value > Encrypt(unencrypted + Data Protection Key + salt) + salt + hash > Base64(encrypted value + Data Protection Key)

            //1. Persist the key ring to a common filesystem or network location path all the processes/apps can access:
            //2. Ensure that all the process/apps are using the same application name:
            services.AddDataProtection()
            .UseCryptographicAlgorithms(
            new AuthenticatedEncryptorConfiguration()
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            })
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
            //.ProtectKeysWithDpapi()  // only the local user account can decrypt the keys
            //.ProtectKeysWithDpapi(protectToLocalMachine: true); all user accounts on the machine can decrypt the keys
            //.PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
            //.PersistKeysToFileSystem(new DirectoryInfo(@"\\server\share\directory\"))
            //.ProtectKeysWithCertificate(new X509Certificate2("certificate.pfx", "password"))
            //.SetApplicationName("web-app"); //salt

            //Load Balanced
            //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

            //var keyBundle = keyVaultClient.GetKeyAsync($"https://{keyVaultName}.vault.azure.net/", "DataProtectionKey")
            //    .ConfigureAwait(false).GetAwaiter().GetResult();
            //var keyIdentifier = keyBundle.KeyIdentifier.Identifier;

            //var storageCredentials = new StorageCredentials(
            //    Configuration.GetValue<string>("DataProtection:StorageAccountName"),
            //    Configuration.GetValue<string>("DataProtection:StorageKey"));
            //var cloudStorageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            //var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            //var cloudBlobContainer = cloudBlobClient.GetContainerReference("dataprotectionkeys");

            // optional - provision the container automatically
            //await container.CreateIfNotExistsAsync();

            //services.AddDataProtection(options => { options.ApplicationDiscriminator = "willow"; })
            //    .PersistKeysToAzureBlobStorage(cloudBlobContainer, "keys.xml") //appends to find instead of creating new each time
            //    .SetApplicationName("Application")
            //    .SetDefaultKeyLifetime(TimeSpan.FromDays(30))
            //    .ProtectKeysWithAzureKeyVault(keyVaultClient, keyIdentifier);
        }
        #endregion

        #region Authentication
        public virtual void ConfigureAuthenticationServices(IServiceCollection services)
        {
            //https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/identity-2x?view=aspnetcore-2.1#cookie-based-authentication
            //Define a default scheme in 2.0 if one of the following conditions is true:
            //You use the [Authorize] attribute or authorization policies without specifying schemes

            //CertificateAuthenticationDefaults.AuthenticationScheme = "Certificate"
            //IdentityConstants.ApplicationScheme = "Identity.Application"
            //IdentityConstants.ExternalScheme = "Identity.External"
            //ExternalAuthenticationDefaults.AuthenticationScheme = "Identity.External"
            //CookieAuthenticationDefaults.AuthenticationScheme = "Cookies"
            //JwtBearerDefaults.AuthenticationScheme = "Bearer"

            Logger.LogInformation("Configuring Authentication");

            var appSettings = GetSettings<AppSettings>("AppSettings");
            var authenticationSettings = GetSettings<AuthenticationSettings>("AuthenticationSettings");
            var tokenSettings = ManipulateTokenSettings(GetSettings<TokenSettings>("TokenSettings"));

            var authenticationBuilder = services.AddAuthentication();

            //Local Jwt Authentication
            if (authenticationSettings.JwtToken.Enable)
            {
                Logger.LogInformation($"Configuring JWT Authentication" + Environment.NewLine +
                                       $"Key:{tokenSettings.Key ?? ""}" + Environment.NewLine +
                                       $"PublicKeyPath: {tokenSettings.PublicKeyPath ?? ""}" + Environment.NewLine +
                                       $"PublicCertificatePath: {tokenSettings.PublicCertificatePath ?? ""}" + Environment.NewLine +
                                       $"ExternalIssuers: {tokenSettings.ExternalIssuers ?? ""}" + Environment.NewLine +
                                       $"LocalIssuer: {tokenSettings.LocalIssuer ?? ""}" + Environment.NewLine +
                                       $"Audiences: {tokenSettings.Audiences ?? ""}");

                services.Configure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                });

                //https://wildermuth.com/2017/08/19/Two-AuthorizationSchemes-in-ASP-NET-Core-2
                authenticationBuilder.AddJwtAuthentication(
                   tokenSettings.Key,
                   tokenSettings.PublicKeyPath,
                   tokenSettings.PublicCertificatePath,
                   tokenSettings.ExternalIssuers,
                   tokenSettings.LocalIssuer,
                   tokenSettings.Audiences);

                if (HostingEnvironment.IsDevelopment())
                    IdentityModelEventSource.ShowPII = true;
            }

            //Reference Token is an Access_Token that doesn't contain claims
            //Remote Jwt Authentication + Ability to get claims via Reference Token
            if (authenticationSettings.OpenIdConnectJwtToken.Enable)
            {
                Logger.LogInformation("Configuring IdentityServer JWT Authentication");

                services.Configure<AuthenticationOptions>(options =>
                {
                    options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                });

                //scheme
                authenticationBuilder
                 .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44318/";
                    options.ApiName = "api";
                    options.ApiSecret = "apisecret"; //Only need this if AccessTokenType = AccessTokenType.Reference
                    options.EnableCaching = true; //Caches response from introspection endpoint.
                });
            }

            //App > IDP > Access Token + RefreshToken > Cookie
            if (authenticationSettings.OpenIdConnect.Enable)
            {
                Logger.LogInformation("Configuring OpenIdConnect");

                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // keep original claim types
                services.Configure<AuthenticationOptions>(options =>
                {
                    //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.1&tabs=aspnetcore2x
                    //overides "Identity.Application"/IdentityConstants.ApplicationScheme set by AddIdentity
                    //Use cookie authentication without ASP.NET Core Identity
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; // Challenge scheme is how user should login if they arent already logged in.
                });

                //authetication scheme seperate to application cookie
                //Use cookie authentication without ASP.NET Core Identity
                authenticationBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, (options) =>
                {
                    options.Cookie.Name = appSettings.CookieAuthName;
                    options.AccessDeniedPath = "Authorization/AccessDenied";
                });

                var authority = tokenSettings.ExternalIssuers.Split(",").First().Trim();
                authenticationBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = authority;

                    //https://www.scottbrady91.com/OpenID-Connect/ASPNET-Core-using-Proof-Key-for-Code-Exchange-PKCE
                    //token = access_token

                    options.ResponseType = "code"; //Authorization** Mitigates injection/substitution attack. Client-side just needs to generate random string and hash it using SHA256.

                    //.NET Core 3.0
                    options.UsePkce = true;

                    //options.ResponseType = "id_token"; //Implicit - Dont Use
                    //options.ResponseType = "id_token token"; //Implicit - Dont Use
                    //options.ResponseType = "code id_token"; //Hybrid MVC/Blazor Server** Mitigates injection/substitution attack but client-side code is more difficult to implement. Potentially leak identifiable information via the front-channel identity token.
                    //options.ResponseType = "code token"; //Hybrid - Dont Use
                    //options.ResponseType = "code id_token token"; //Hybrid - Dont Use

                    //token = access_token
                    //id_token = identity_token (by default doesnt include claims)
                    //code > token (access_token)

                    //options.CallbackPath = new PathString("...")
                    //options.SignedOutCallbackPath = new PathString("...")
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("address");
                    options.Scope.Add("roles");

                    options.Scope.Add("api"); //ApiResource.Name if 0 scopes else Scope.Name

                    options.Scope.Add("subscriptionlevel");
                    options.Scope.Add("country");
                    options.Scope.Add("offline_access"); //refresh tokens, Enabled by AllowOfflineAccess = true. An alternative is AccessTokenType=Reference which allows access to removed.

                    //Saves Access and Refresh Tokens in cookie for later use. Can call HttpContext.GetTokenAsync("access_token").
                    //Allows for Blazor/SPA > Server > API
                    options.SaveTokens = true;

                    options.ClientId = "mvc_client";
                    options.ClientSecret = "secret";
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.ClaimActions.Remove("amr"); //keep claim
                    options.ClaimActions.DeleteClaim("sid"); //delete claim
                    options.ClaimActions.DeleteClaim("idp"); //delete claim
                    options.ClaimActions.DeleteClaim("s_hash"); //delete claim
                    options.ClaimActions.DeleteClaim("auth_time"); //delete claim

                    options.ClaimActions.MapUniqueJsonKey("role", "role");
                    options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                    options.ClaimActions.MapUniqueJsonKey("country", "country");

                    //For serialization
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = JwtClaimTypes.GivenName,
                        RoleClaimType = JwtClaimTypes.Role,
                    };

                    // https://docs.microsoft.com/en-us/dotnet/api/
                    // microsoft.aspnetcore.authentication.openidconnect.openidconnectevents
                    options.Events.OnTicketReceived = e =>
                    {
                        Log.Information("Login successfully completed for {UserName}.",
                            e.Principal.Identity.Name);
                        return Task.CompletedTask;
                    };
                });

                services.AddHttpClient("IDPClient", client =>
                {
                    client.BaseAddress = new Uri(authority);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                });

                services.AddSingleton<IOpenIDConnectService>(sp =>
                new OpenIDConnectService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("IDPClient"),
                    clientId: "mvc_client",
                    responseType: "code", //Authorization Code + PKCE
                    scopes: "openid profile email address roles api subscriptionlevel country offline_access")
                );
            }

            if (authenticationSettings.Google.Enable)
            {
                //If not using Microsoft.AspNetCore.Identity
                //var authenticationBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
                //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
                //.AddCookie(ExternalAuthenticationDefaults.AuthenticationScheme);

                authenticationBuilder.AddGoogle("Google", options =>
                {
                    options.ClientId = authenticationSettings.Google.ClientId;
                    options.ClientSecret = authenticationSettings.Google.ClientSecret;
                    options.SignInScheme = ExternalAuthenticationDefaults.AuthenticationScheme; //IdentityConstants.ExternalScheme when Microsoft.AspNetCore.Identity or ExternalAuthenticationDefaults.AuthenticationScheme when not
                });
            }

            if (authenticationSettings.Facebook.Enable)
            {
                //If not using Microsoft.AspNetCore.Identity
                //var authenticationBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
                //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
                //.AddCookie(ExternalAuthenticationDefaults.AuthenticationScheme);

                authenticationBuilder.AddFacebook("Facebook", options =>
                {
                    options.ClientId = authenticationSettings.Facebook.ClientId;
                    options.ClientSecret = authenticationSettings.Facebook.ClientSecret;
                    options.SignInScheme = ExternalAuthenticationDefaults.AuthenticationScheme; //IdentityConstants.ExternalScheme when Microsoft.AspNetCore.Identity or ExternalAuthenticationDefaults.AuthenticationScheme when not
                });
            }

            //.NET Core 3.0
            //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-3.1
            //https://www.yogihosting.com/certificate-authentication/?fbclid=IwAR2eD_D160KR7GnxKN2UrOY0URPOWrTGVt-2ug_CJXg8P9mFhxu52fhfC6U
            services.AddCertificateForwarding(options =>
            {
                options.CertificateHeader = "X-SSL-CERT";
                options.HeaderConverter = (headerValue) =>
                {
                    X509Certificate2 clientCertificate = null;

                    if (!string.IsNullOrWhiteSpace(headerValue))
                    {
                        byte[] bytes = StringToByteArray(headerValue);
                        clientCertificate = new X509Certificate2(bytes);
                    }

                    return clientCertificate;
                };
            });

            static byte[] StringToByteArray(string hex)
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];

                for (int i = 0; i < NumberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return bytes;
            }

            authenticationBuilder.AddCertificate(options =>
             {
                 options.Events = new CertificateAuthenticationEvents
                 {
                     OnCertificateValidated = context =>
                     {
                         var listOfValidThumbprints = new List<string>
                         {

                         };

                         if (listOfValidThumbprints.Contains(context.ClientCertificate.Thumbprint))
                         {
                             var claims = new Claim[] { };
                             context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                             context.Success();
                         }
                         else
                         {
                             context.Fail("invalid cert");
                         }

                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = context =>
                     {
                         context.Fail("invalid cert");
                         return Task.CompletedTask;
                     }
                 };
             });
        }

        #endregion

        #region Authorization
        public virtual void ConfigureAuthorizationServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Authorization");

            var authorizationSettings = GetSettings<AuthorizationSettings>("AuthorizationSettings");

            //Add this to controller or action using Authorize(Policy = "UserMustBeAdmin")
            //Can create custom requirements by implementing IAuthorizationRequirement and AuthorizationHandler (Needs to be added to services as scoped)
            services.AddAuthorization(options =>
            {
                //https://ondrejbalas.com/authorization-options-in-asp-net-core/
                //The default policy will only be executed on requests prior to entering protected actions such as those wrapped by an [Authorize] attribute or endpoints with RequireAuthorization().
                //The DefaultPolicy is initially configured to require authentication, so no additional configuration is actually required. In the following example, MVC endpoints are marked as RequireAuthorization so that all requests must be authorized based on the DefaultPolicy.
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                //other requirements suchas account open

                //FallbackPolicy replaces  AuthorizeFilter as a global filter in MVC. This applies to all endpoints which don't have any authorization.
                if (authorizationSettings.UserMustBeAuthorizedByDefault)
                {
                    //.NET Core 3.0
                    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
                }

                options.AddPolicy("UserMustBeAdmin", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireRole("Admin");
                    //policyBuilder.AddRequirements();
                });

                options.AddPolicy("HealthCheckPolicy", policyBuilder =>
                {
                    //policyBuilder.RequireClaim("client_policy", "healthChecks");
                    policyBuilder.AllowAnonymous();
                });
            });

            //https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-2.1&tabs=aspnetcore2x
            services.AddSingleton<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, AnonymousAuthorizationHandler>();
            //Scope name as policy
            //https://www.jerriepelser.com/blog/creating-dynamic-authorization-policies-aspnet-core/
            services.AddSingleton<IAuthorizationPolicyProvider, ScopeAndAnonymousAuthorizationPolicyProvider>();
            //services.AddSingleton<IAuthorizationPolicyProvider, ScopeAuthorizationPolicyProvider>();
        }
        #endregion

        #region Security
        public virtual void ConfigureSecurityServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Security");

            var switchSettings = GetSettings<SwitchSettings>("SwitchSettings");
            var corsSettings = GetSettings<CORSSettings>("CORSSettings");

            services.AddAntiforgery(o =>
            {
                o.SuppressXFrameOptionsHeader = switchSettings.EnableIFramesGlobal;
                //o.Cookie.Name = "X-XSRF-TOKEN-COOKIE";//The default cookie name is generated for each application as “.AspNetCore.AntiForgery.“ followed by a hash of the application name. It can be overridden by providing a CookieName in the options.
                // Angular's default header name for sending the XSRF token.

                //APIS are vulnerable to CSRF attack as long as the server uses authenticated session(cookies).
                //The solution is
                //1.Ensure that the 'safe' HTTP operations, such as GET, HEAD, OPTIONS, TRACE cannot be used to alter any server-side state.
                //2.Ensure that any 'unsafe' HTTP operations, such as POST, PUT, PATCH and DELETE, always require a valid CSRF token!

                //Although example1.contoso.net and example2.contoso.net are different hosts, there's an implicit trust relationship between hosts under the *.contoso.net domain. This implicit trust relationship allows potentially untrusted hosts to affect each other's cookies(the same-origin policies that govern AJAX requests don't necessarily apply to HTTP cookies).
                //Attacks that exploit trusted cookies between apps hosted on the same domain can be prevented by not sharing domains.When each app is hosted on its own domain, there is no implicit cookie trust relationship to exploit.


                o.HeaderName = "X-XSRF-TOKEN";
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            });

            services.ConfigureCorsAllowAnyOrigin("AllowAnyOrigin");
            services.ConfigureCorsAllowSpecificOrigin("AllowSpecificOrigin", corsSettings.Domains);
        }
        #endregion

        #region Email
        public virtual void ConfigureEmailServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Email");

            var emailSettings = GetSettings<EmailSettings>("EmailSettings");

            if (emailSettings.SendEmailsLive)
            {
                if (!string.IsNullOrEmpty(emailSettings.SendGridApiKey))
                {
                    services.AddTransient<IEmailService, EmailServiceSendGrid>();
                }
                else
                {
                    services.AddTransient<IEmailService, EmailServiceSmtp>();
                }
            }
            else
            {
                services.AddTransient<IEmailService, EmailServiceFileSystem>();
            }
        }
        #endregion

        #region Caching
        public virtual void ConfigureCachingServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Caching");

            var appSettings = GetSettings<AppSettings>("AppSettings");
            services.AddResponseCaching(options =>
            {
                options.SizeLimit = appSettings.ResponseCacheSizeMB * 1024 * 1024; //100Mb
                options.MaximumBodySize = 64 * 1024 * 1024; //64Mb
            });


            services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            //https://stackoverflow.com/questions/46492736/asp-net-core-2-0-http-response-caching-middleware-nothing-cached
            //Client Side Cache Time
            //services.AddHttpCacheHeaders(opt => opt.MaxAge = 600, opt => opt.MustRevalidate = true);
            services.AddHttpCacheHeaders();
        }
        #endregion

        #region Compression
        public virtual void ConfigureResponseCompressionServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Response Compression");

            //Compression
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                //options.Providers.Add<GzipCompressionProvider>();
                //options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { ""});
            });

            //services.Configure<GzipCompressionProviderOptions>(options =>
            //{
            //    options.Level = CompressionLevel.Fastest;
            //});

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
        }
        #endregion

        #region Localization
        public virtual void ConfigureLocalizationServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Localization");

            var localizationSettings = GetSettings<LocalizationSettings>("LocalizationSettings");

            //https://github.com/RickStrahl/Westwind.Globalization

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddUrlLocalization();

            services.AddCultureRouteConstraint("cultureCheck");

            services.AddRequestLocalizationOptions(
                localizationSettings.DefaultCulture,
                localizationSettings.SupportAllCountriesFormatting,
                localizationSettings.SupportAllLanguagesFormatting,
                localizationSettings.SupportUICultureFormatting,
                localizationSettings.SupportDefaultCultureLanguageFormatting,
                localizationSettings.SupportedUICultures);

            services.Configure<RedirectUnsupportedUrlCulturesOptions>(options =>
            {
                options.RedirectUnspportedCulturesToDefaultCulture = true;
                options.RedirectCulturelessToDefaultCulture = localizationSettings.AlwaysIncludeCultureInUrl;
            });

            services.AddSingleton(sp => sp.GetService<IOptions<RedirectUnsupportedUrlCulturesOptions>>().Value);

            //CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            //Globalization(G11N): The process of making an app support different languages and regions.
            //Localization(L10N): The process of customizing an app for a given language and region.
            //Internationalization(I18N): Describes both globalization and localization.
            //Culture: It's a language and, optionally, a region.
            //Neutral culture: A culture that has a specified language, but not a region. (for example "en", "es")
            //                    Specific culture: A culture that has a specified language and region. (for example "en-US", "en-GB", "es-CL")
            //                Parent culture: The neutral culture that contains a specific culture. (for example, "en" is the parent culture of "en-US" and "en-GB")
            //            Locale: A locale is the same as a culture.
        }
        #endregion

        #region Mvc
        public virtual void ConfigureMvcServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Mvc");

            var appSettings = GetSettings<AppSettings>("AppSettings");
            var localizationSettings = GetSettings<LocalizationSettings>("LocalizationSettings");
            var switchSettings = GetSettings<SwitchSettings>("SwitchSettings");
            var authorizationSettings = GetSettings<AuthorizationSettings>("AuthorizationSettings");

            //settings will automatically be used by JsonConvert.SerializeObject/DeserializeObject.
            var defaultSettings = new JsonSerializerSettings()
            {
                DateParseHandling = DateParseHandling.DateTime, //Only for JObjects.
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind, //Parsing. Dates ending with Z will become Kind = UTC, Dates with timezone +03:00 will get converted to Local, else unspecified.
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                Converters = new List<Newtonsoft.Json.JsonConverter>() { new Newtonsoft.Json.Converters.StringEnumConverter() },
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };

            JsonConvert.DefaultSettings = () => defaultSettings;

            //https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/

            //.NET Core 3.0
            //services.AddControllers(); //Controllers
            //services.AddRazorPages(); //Razor Pages, Included in AddMvc
            //services.AddControllersWithViews() //Controllers + Views, Included in AddMvc

            //Dynamic Json not supported until 5.0
            //https://github.com/dotnet/runtime/issues/29690
            //https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to
            services.Configure<JsonOptions>(options =>
            {
                //By default System.Text.Json is case sensitive.JSON.NET is default case insenstive.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.WriteIndented = defaultSettings.Formatting == Formatting.Indented;
                options.JsonSerializerOptions.IgnoreNullValues = defaultSettings.DefaultValueHandling == DefaultValueHandling.Ignore;
                options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                //options.JsonSerializerOptions.Converters.Add(new DynamicJsonConverter()); //dynamic serialization/deserialization seems to work fine in 3.1
            });

            // Workaround: https://github.com/dotnet/runtime/issues/31094#issuecomment-543342051
            //JsonSerializer.Serialize
            //POCO a = JsonSerializer.Deserialize<POCO>(jsonString) or object o = JsonSerializer.Deserialize<dynamic>(jsonString)
            var defaultJsonSerializerOptions = ((JsonSerializerOptions)typeof(JsonSerializerOptions).GetField("s_defaultOptions", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null));
            defaultJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            defaultJsonSerializerOptions.WriteIndented = defaultSettings.Formatting == Formatting.Indented;
            defaultJsonSerializerOptions.IgnoreNullValues = defaultSettings.DefaultValueHandling == DefaultValueHandling.Ignore;
            defaultJsonSerializerOptions.IgnoreReadOnlyProperties = false;
            defaultJsonSerializerOptions.PropertyNameCaseInsensitive = true;
            defaultJsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            defaultJsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

            var mvc = services.AddMvc(options =>
            {

                //.NET Core 2.2 - Versioning Fix until .NET Core 3.0
                //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
                //options.EnableEndpointRouting = false;

                //.NET Core 2.2
                //https://github.com/aspnet/Security/issues/1764
                //options.AllowCombiningAuthorizeFilters = false;

                //options.Filters.Add<ApiGenerateAntiForgeryTokenAttribute>();

                //Adds {culture=default} to ALL routes
                if (localizationSettings.AlwaysIncludeCultureInUrl)
                {
                    options.AddCultureAttributeRouteConvention();
                }
                else
                {
                    options.AddOptionalCultureAttributeRouteConvention();
                }

                options.Conventions.Add(new MvcAsApiConvention(o =>
                {
                    o.ApplyToApiControllerActions = false; //This allows Swagger to show Json Body field rather than query string.

                    o.DisableAntiForgeryForApiRequestsInDevelopmentEnvironment = true;
                    o.DisableAntiForgeryForApiRequestsInAllEnvironments = false;
                    o.MvcErrorOptions = (mvcErrorOptions) =>
                    {

                    };
                    o.MvcExceptionOptions = (mvcExceptionOptions) =>
                    {
                        mvcExceptionOptions.ActionResultFactories.Add(typeof(UnauthorizedErrors), (context, exception, logger) =>
                        {
                            return context.HttpContext.User.Identity.IsAuthenticated ? new ForbidResult() : (IActionResult)new ChallengeResult();
                        });
                    };
                    o.ApiErrorOptions = (apiErrorOptions) =>
                    {

                    };
                    o.ApiExceptionOptions = (apiExceptionOptions) =>
                    {
                        apiExceptionOptions.ActionResultFactories.Add(typeof(UnauthorizedErrors), (context, exception, logger) =>
                        {
                            return context.HttpContext.User.Identity.IsAuthenticated ? new StatusCodeResult(StatusCodes.Status403Forbidden) : new UnauthorizedResult();
                        });
                        apiExceptionOptions.ActionResultFactories.Add(typeof(BadRequestError), (context, exception, logger) =>
                        {
                            return new ExceptionResult(exception, StatusCodes.Status400BadRequest);
                        });
                    };
                }));

                //Middleware Pipeline - Wraps MVC
                //options.Filters.Add(new MvcUrlLocalizationFilterAttribute());

                //Action Pipeline - Wraps Actions within MVC
                options.Filters.Add<SerilogLoggingActionFilter>();
                options.Filters.Add<SerilogLoggingPageFilter>();
                //options.Filters.Add<AutoValidateFormAntiforgeryTokenAttribute>();
                options.Filters.Add<ValidatableAttribute>();
                //options.Filters.Add<OperationCancelledExceptionFilterAttribute>();

                //[FromBody] or [ApiController]
                //https://stackoverflow.com/questions/31952002/asp-net-core-mvc-how-to-get-raw-json-bound-to-a-string-without-a-type
                //options.InputFormatters.Insert(0, new RawStringRequestBodyInputFormatter());
                //options.InputFormatters.Insert(0, new RawBytesRequestBodyInputFormatter());

                //options.Filters.Add(typeof(ModelValidationFilter));
                ConfigureMvcCachingProfiles(options);
            })
            .AddRazorPagesOptions(options => {
                if (localizationSettings.AlwaysIncludeCultureInUrl)
                {
                    options.AddCultureAttributeRouteConvention();
                }
                else
                {
                    options.AddOptionalCultureAttributeRouteConvention();
                }
            })

            //.NET Core 2.2
            //.AddJsonOptions(options =>
            //{
            //    //https://github.com/aspnet/Mvc/blob/32e21e2a5c63e20bd62b9c1699932207b962fc50/src/Microsoft.AspNetCore.Mvc.Formatters.Json/JsonSerializerSettingsProvider.cs#L31-L41
            //    options.SerializerSettings.ReferenceLoopHandling = defaultSettings.ReferenceLoopHandling;
            //    options.SerializerSettings.Formatting = defaultSettings.Formatting;
            //    options.SerializerSettings.Converters = defaultSettings.Converters;
            //    options.SerializerSettings.ContractResolver = defaultSettings.ContractResolver;
            //    options.SerializerSettings.DefaultValueHandling = defaultSettings.DefaultValueHandling;
            //    options.SerializerSettings.NullValueHandling = defaultSettings.NullValueHandling;
            //    options.SerializerSettings.MissingMemberHandling = defaultSettings.MissingMemberHandling;
            //    options.SerializerSettings.TypeNameHandling = defaultSettings.TypeNameHandling;
            //})

            //.NET Core 3.0
            //This sets up MVC and configures it to use Json.NET instead of that new API(System.Text.Json). AddNewtonsoftJson method has an overload that allows you to configure the Json.NET options like you were used to with AddJsonOptions in ASP.NET Core 2.x
            //.AddNewtonsoftJson(options =>
            //{
            //    //using Microsoft.AspNetCore.Mvc.NewtonsoftJson
            //    //https://github.com/aspnet/Mvc/blob/32e21e2a5c63e20bd62b9c1699932207b962fc50/src/Microsoft.AspNetCore.Mvc.Formatters.Json/JsonSerializerSettingsProvider.cs#L31-L41
            //    options.SerializerSettings.ReferenceLoopHandling = defaultSettings.ReferenceLoopHandling;
            //    options.SerializerSettings.Formatting = defaultSettings.Formatting;
            //    options.SerializerSettings.Converters = defaultSettings.Converters;
            //    options.SerializerSettings.ContractResolver = defaultSettings.ContractResolver;
            //    options.SerializerSettings.DefaultValueHandling = defaultSettings.DefaultValueHandling;
            //    options.SerializerSettings.NullValueHandling = defaultSettings.NullValueHandling;
            //    options.SerializerSettings.MissingMemberHandling = defaultSettings.MissingMemberHandling;
            //    options.SerializerSettings.TypeNameHandling = defaultSettings.TypeNameHandling;
            //})

            .AddXmlDataContractSerializerFormatters() //Can serializer datetimeoffset. DataContractSerializer is generally opt-in except for public properties. Should go after AddNewtonsoftJson.
            .SetCompatibilityVersion(CompatibilityVersion.Latest)
            .AddFeatureFolders(options =>
             {
                 options.SharedViewFolders = new List<string>() {
                    "Bundles",
                    "Sidebar",
                    "CRUD",
                    "Navigation",
                    "Footer",
                    "Alerts",
                    "CookieConsent"
                    };
             })
            .AddAreaFeatureFolders(options =>
            {
                options.SharedViewFolders = new List<string>() {
                    "Bundles",
                    "Sidebar",
                    "CRUD",
                    "Navigation",
                    "Footer",
                    "Alerts",
                    "CookieConsent"
                };
            })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization()
            .AddActionLinkLocalization()
            .AddAmbientRouteDataUrlHelperFactory(options =>
            {
                //options.AmbientRouteDataKeys.Add(new AmbientRouteDataKey("area", false));
                //options.AmbientRouteDataKeys.Add(new AmbientRouteDataKey("version", false));
                //options.AmbientRouteDataKeys.Add(new AmbientRouteDataKey("culture", true));
                //options.AmbientRouteDataKeys.Add(new AmbientRouteDataKey("ui-culture", true));
                options.AmbientRouteDataKeys.Add(new AmbientRouteDataKey("tenant", true));
            })
            //By default, ASP.NET Core will resolve the controller parameters from the container but doesn’t actually resolve the controller from the container.
            //https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
            //The AddControllersAsServices method does two things - it registers all of the Controllers in your application with the DI container as Transient (if they haven't already been registered) and replaces the IControllerActivator registration with the ServiceBasedControllerActivator
            .AddControllersAsServices()
            .AddMvcUserSpecificationModelBinders()
            .AddMvcPointModelBinder()
            .SuppressMvcPointChildValidation()
            .AddViewOptions(options =>
             {
                 if (HostingEnvironment.IsDevelopment())
                 {
                     options.HtmlHelperOptions.ClientValidationEnabled = switchSettings.EnableClientValidation;
                 }
             })

            //.NET Core 2.2
            //.UserMustBeAuthorized(authorizationSettings.UserMustBeAuthorizedByDefault) //The authorization filter will be executed on any request that enters the MVC middleware and maps to a valid MVC action.

            .AddMvcDisplayConventions(
            new AppendAsterixToRequiredFieldLabels((viewContext) => ((viewContext.ViewData.ContainsKey("EditMode") && (Boolean)viewContext.ViewData["EditMode"]) || (viewContext.ViewData.ContainsKey("CreateMode") && (Boolean)viewContext.ViewData["CreateMode"])) && !(viewContext.ViewData.ContainsKey("DetailsMode") && (Boolean)viewContext.ViewData["DetailsMode"])),
            new HtmlByNameConventionFilter(),
            new LabelTextConventionFilter(),
            new TextAreaByNameConventionFilter(),
            new TextboxPlaceholderConventionFilter())
            .AddMvcValidationConventions()
            .AddMvcDisplayAttributes()
            .AddMvcInheritanceValidationAttributeAdapterProvider()
            .AddMvcDynamicModelBinder()
            .AddMvcViewRendererAndPdfGenerator()
            .AddMvcSelectListAttributes()
            .AddMvcFeatureService()
            .AddMvcJsonNavigationService(options =>
            {
                options.FileNames.Add("navigation-admin.json");
            })
            .AddMvcRawBytesRequestBodyInputFormatter()
            //.NET Core 3.0 using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
#if DEBUG
            //Only need this in Development
            .AddRazorRuntimeCompilation() //By default uses DefaultViewCompilerProvider > RuntimeViewCompilerProvider
#endif
            .AddUrlHelperService()
            .AddFluentValidation(options => options.RegisterValidatorsFromAssemblies(ApplicationParts));

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.Name = appSettings.CookieTempDataName;
            });

            //https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-2.2&tabs=visual-studio
            //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Razor/src/Compilation/DefaultViewCompilerProvider.cs
            //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RuntimeViewCompilerProvider.cs
            //https://github.com/aspnet/AspNetCore/blob/1b500858354efe26493af632bf0e3f5462dc6246/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RuntimeViewCompiler.cs         

            services.AddFluentMetadata();

            services.AddCookiePolicy(appSettings.CookieConsentName);

            var test = new TemplateInfo();

            //singleton
            //services.AddHttpContextAccessor();

            //Using AddFeatureFolders and AddAreaFeatureFolders instead.
            //services.AddViewLocationExpander(appSettings.MvcImplementationFolder);

            services.AddBundleConfigService();

            ConfigureMvcModelValidation(mvc, services);
            ConfigureMvcApplicationParts(mvc, services);
            ConfigureMvcRouting(services);
        }

        public virtual void ConfigureMvcCachingProfiles(MvcOptions options)
        {
            //Cache-control: no-cache = store response on client browser but recheck with server each request 
            //Cache-control: no-store = dont store response on client
            options.CacheProfiles.Add("Cache24HourNoParams", new CacheProfile()
            {
                VaryByHeader = "Accept,Accept-Language,X-Requested-With",
                //VaryByQueryKeys = "", Only used for server side caching
                Duration = 60 * 60 * 24, // 24 hour,
                Location = ResponseCacheLocation.Any,// Any = Cached on Server, Client and Proxies. Client = Client Only
                NoStore = false
            });

            options.CacheProfiles.Add("Cache24HourParams", new CacheProfile()
            {
                //IIS DynamicCompressionModule and StaticCompressionModule add the Accept-Encoding Vary header.
                VaryByHeader = "Accept,Accept-Language,X-Requested-With",
                VaryByQueryKeys = new string[] { "*" }, //Only used for server side caching
                Duration = 60 * 60 * 24, // 24 hour,
                Location = ResponseCacheLocation.Any,// Any = Cached on Server, Client and Proxies. Client = Client Only
                NoStore = false
            });
        }

        public virtual void ConfigureMvcRouting(IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("promo", typeof(PromoConstraint));
                options.ConstraintMap.Add("tokenCheck", typeof(TokenConstraint));
                options.ConstraintMap.Add("versionCheck", typeof(RouteVersionConstraint));
                //options.ConstraintMap.Add("cultureCheck", typeof(CultureConstraint));
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        }

        public virtual void ConfigureMvcModelValidation(IMvcBuilder mvcBuilder, IServiceCollection services)
        {
            Logger.LogInformation("Configuring Mvc Model Validation");

            var switchSettings = GetSettings<SwitchSettings>("SwitchSettings");

            //Disable IObjectValidatable and Validation Attributes from being evaluated and populating modelstate
            //https://stackoverflow.com/questions/46374994/correct-way-to-disable-model-validation-in-asp-net-core-2-mvc
            if (!switchSettings.EnableMVCModelValidation)
            {
                mvcBuilder.DisableModelValidation();
            }
        }

        public virtual void ConfigureMvcApplicationParts(IMvcBuilder mvcBuilder, IServiceCollection services)
        {
            //Logger.LogInformation("Configuring Application Parts");

            //https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-3.1
            //https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts
            //An Application Part is an abstraction over the resources of an MVC app. Application Parts allow ASP.NET Core to discover controllers, view components, tag helpers, Razor Pages, razor compilation sources, and more
            //Note that in ASP.NET Core 3.x, when you compile a root web assembly (Microsoft.NET.Sdk.Web), assembly attribute [ApplicationPart] are added to the output for each dependency that references (direct or transitive) Microsoft.AspNetCore.Mvc. ASP.NET Core 3.x apps look for this attribute and registers them as application parts automatically.
            //<AddRazorSupportForMvc>true</AddRazorSupportForMvc> generates the views dll when Build Action: Content, Copy to Output Directory: None.
            //Build Action: EmbeddedResource does not work!

            //AddMvcCore > PopulateDefaultParts > scans entry assembly for [ApplicationPart].

            //Not required when using Razor Class Libraries!
            //Add Controllers and views from other assemblies
            //foreach (var assembly in ApplicationParts)
            //{
            //    mvcBuilder.AddApplicationPart(assembly).AddControllersAsServices();
            //}

            //Not required when using Razor Class Libraries!
            //.NET Core 3.0
            //Add Embedded views from other assemblies
            //services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            //{
            //    //Add Embedded Views from other assemblies.
            //    //Edit and Continue wont work with these views.
            //    foreach (var assembly in ApplicationParts)
            //    {
            //        options.FileProviders.Add(new EmbeddedFileProvider(assembly));
            //    }

            //    //https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.1&tabs=visual-studio
            //    //Admin Views
            //    options.FileProviders.Add(new EmbeddedFileProvider(typeof(DropdownDbContextAttribute).Assembly));
            //});

            //Not required when using Razor Class Libraries!
            //.NET Core 2.2
            //Add Embedded views from other assemblies
            //services.Configure<RazorViewEngineOptions>(options =>
            //{
            //    //Add Embedded Views from other assemblies
            //    //Edit and Continue wont work with these views.
            //    foreach (var assembly in ApplicationParts)
            //    {
            //        options.FileProviders.Add(new EmbeddedFileProvider(assembly));
            //    }

            //    options.FileProviders.Add(new EmbeddedFileProvider(typeof(DropdownDbContextAttribute).Assembly));
            //});
        }
        #endregion

        #region SPA
        public virtual void ConfigureSpaServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Spa");

            var appSettings = GetSettings<AppSettings>("AppSettings");

            if (Options.ReactApp)
            {
                // In production, the React files will be served from this directory
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/build";
                });
            }
            else if (Options.AngularApp)
            {
                // In production, the Angular files will be served from this directory
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/dist";
                });
            }
        }
        #endregion

        #region Validation
        public virtual void ConfigureValidationServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Validation");
            services.AddValidationService();
        }
        #endregion

        #region Contexts
        public virtual void ConfigureContextServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Context Services");
            services.AddTransient<ControllerServicesContext>();
            services.AddTransient<ApplicationervicesContext>();
        }
        #endregion

        #region Blazor
        //https://chrissainty.com/using-blazor-components-in-an-existing-mvc-application/?fbclid=IwAR3o7t0MhhQnSsMWrPxks-1xXxN9-4TZcUP6ntjhNXR8Ili_pQ8boT06nCY
        public virtual void ConfigureBlazorServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Blazor");

            //<script src="_framework/blazor.server.js"></script> Add to _Layout.cshtml or _Host.cshtml
            services.AddServerSideBlazor();
        }
        #endregion

        #region Events
        public virtual void ConfigureEventServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Events");

            services.AddCqrs(ApplicationParts);
            services.AddHangFireDomainEvents(ApplicationParts);
            services.AddHangFireIntegrationEvents(ApplicationParts);
        }
        #endregion

        #region SignalR
        public virtual void ConfigureSignalRServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring SignalR");

            var defaultSettings = JsonConvert.DefaultSettings();

            //Microsoft.AspNetCore.SignalR.Protocols.MessagePack
            //MessagePack is a binary serialization format that is fast and compact. It's useful when performance and bandwidth are a concern because it creates smaller messages compared to JSON
            var builder = services.AddSignalR().AddMessagePackProtocol();

            builder.AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.WriteIndented = defaultSettings.Formatting == Formatting.Indented;
                options.PayloadSerializerOptions.IgnoreNullValues = defaultSettings.DefaultValueHandling == DefaultValueHandling.Ignore;
                options.PayloadSerializerOptions.IgnoreReadOnlyProperties = false;
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddSignalRHubMapper(options =>
            {
                options.LoadApplicationDependencies = false;
                options.Predicate = AssemblyBoolFilter;
            });
        }
        #endregion

        #region Api
        public virtual void ConfigureApiServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Api");

            var appSettings = GetSettings<AppSettings>("AppSettings");

            var builder = services.AddMvc(options =>
            {
                //https://weblog.west-wind.com/posts/2020/Feb/24/Null-API-Responses-and-HTTP-204-Results-in-ASPNET-Core?fbclid=IwAR1JjFn12EkBl8k_ie8XOlZe_5KgiRLk11zOkrIB63_LExppY-RPyGzo17c
                //https://github.com/aspnet/AspNetCore/issues/8847
                //By default null responses return 204 rather than null
                var noContentFormatter = options.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
                if (noContentFormatter != null)
                {
                    noContentFormatter.TreatNullValueAsNoContent = Options.TreatNullValueAsNoContent;
                }
            });

            //Overrides the default IClientErrorFactory implementation which adds traceId, timeGenerated and exception details to the ProblemDetails response.
            builder.AddMvcProblemDetailsClientErrorAndExceptionFactory(options => options.ShowExceptionDetails = HostingEnvironment.IsDevelopment() || HostingEnvironment.IsIntegration());
            //Overrides the default InvalidModelStateResponseFactory, adds traceId and timeGenerated to the ProblemDetails response. 
            builder.ConfigureMvcProblemDetailsInvalidModelStateFactory(options => options.EnableAngularErrors = true);

            builder.AddApiVersioning();
            builder.AddSwaggerWithApiVersioning();
            builder.ConfigureMvcVariableResourceRepresentations();

            //API rate limiting
            //services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>((options) =>
            {
                options.GeneralRules = new List<RateLimitRule>()
                {
                    new RateLimitRule()
                    {
                        Endpoint = "*",
                        Limit = 3,
                        Period = "5m"
                    },
                     new RateLimitRule()
                    {
                        Endpoint = "*",
                        Limit = 2,
                        Period = "10s"
                    }
                };
            });
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        }
        #endregion

        #region Health Checks

        //AspNetCore.HealthChecks.SqlServer
        //AspNetCore.HealthChecks.Uris
        //AspNetCore.HealthChecks.UI
        public virtual void ConfigureHealthCheckServices(IServiceCollection services)
        {
            //live health checks
            //readyness health checks
            //use tags to group health checks

            var builder = services.AddHealthChecks()
                .AddSystemMemoryCheck();
            //.AddCheck("SQL Check", () =>
            //{
            //    using (var conn = new SqlConnection(""))
            //    {
            //        try
            //        {
            //            return HealthCheckResult.Healthy();
            //        }
            //        catch (SqlException)
            //        {
            //            return HealthCheckResult.Unhealthy();
            //        }
            //    }
            //});

            AddHealthChecks(builder);
        }
        #endregion

        #region Elastic
        public virtual void ConfigureElasticServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring ElasticSearch");

            var elasticSettings = GetSettings<ElasticSettings>("ElasticSettings");
            if (!string.IsNullOrWhiteSpace(elasticSettings.Uri))
            {
                services.AddElasticSearch(elasticSettings.Uri, elasticSettings.DefaultIndex);
            }
        }
        #endregion

        #region GraphQL
        public virtual void ConfigureGraphQL(IServiceCollection services)
        {
            Logger.LogInformation("Configuring GraphQL");

            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

            services.AddGraphQL(o => { o.ExposeExceptions = HostingEnvironment.IsDevelopment(); })
              .AddGraphTypes(ServiceLifetime.Scoped)
              .AddUserContextBuilder(httpContext => httpContext.User)
              .AddDataLoader()
              .AddWebSockets();

        }
        #endregion

        #region HttpClients
        //https://www.codemag.com/article/1807041/What%E2%80%99s-New-in-ASP.NET-Core-2.1
        public virtual void ConfigureHttpClients(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Http Clients");

            services.AddTransient<ProfilingHttpHandler>();
            services.AddTransient<AuthorizationProxyHttpHandler>();
            services.AddTransient<BearerHttpHandler>();
            services.AddTransient<BearerRefreshHttpHandler>();
            services.AddTransient<RetryPolicyDelegatingHttpHandler>();
            services.AddTransient<TimeOutDelegatingHttpHandler>();

            AddHttpClients(services);

            //JSON requests and responses are serialized/deserialized using Json.NET. By default will use the serializer settings that can be configured by setting Newtonsoft.Json.JsonConvert.DefaultSettings:

            //using Microsoft.Extensions.Http
            //When using typed client its best to put client config in the constructor.
            //services.AddHttpClient<IClient, Client>()
            //     .AddHttpMessageHandler(handler => new RetryPolicyDelegatingHandler(2))
            //     .AddHttpMessageHandler(handler => new TimeOutDelegatingHandler(TimeSpan.FromSeconds(20)))
            //    .AddHttpMessageHandler<ProfilingHttpHandler>()
            //    .AddHttpMessageHandler<AuthorizationBearerProxyHttpHandler>()
            //    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            //{
            //    ClientCertificateOptions = ClientCertificateOption.Manual,
            //    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true,
            //    AllowAutoRedirect = true,
            //    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli
            //});

            //services.AddHttpClient("name")
            //     .AddHttpMessageHandler(handler => new RetryPolicyDelegatingHandler(2))
            //     .AddHttpMessageHandler(handler => new TimeOutDelegatingHandler(TimeSpan.FromSeconds(20)))
            //     .AddHttpMessageHandler<ProfilingHttpHandler>()
            //     .AddHttpMessageHandler<AuthorizationJwtProxyHttpHandler>()
            //    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            //    {
            //        ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true,
            //        AllowAutoRedirect = true,
            //        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli
            //    });

            // services.AddHttpClient(typeof(IClient).Name)
            // .ConfigureHttpClient(c =>
            // {
            //     c.BaseAddress = new Uri("http://localhost:5000");
            // })
            //.AddTypedClient<IClient>((httpClient, sp) =>
            // {
            //      //return implementation
            //      var defaultSettings = new JsonSerializerSettings()
            //     {
            //         ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //         Formatting = Formatting.Indented,
            //         Converters = new List<JsonConverter>() { new Newtonsoft.Json.Converters.StringEnumConverter() },
            //         ContractResolver = new CamelCasePropertyNamesContractResolver(),
            //         DefaultValueHandling = DefaultValueHandling.Include,
            //         NullValueHandling = NullValueHandling.Include,
            //         MissingMemberHandling = MissingMemberHandling.Ignore,
            //         TypeNameHandling = TypeNameHandling.None
            //     };

            //     return new Client(httpClient, serializerSettings);
            //     return Refit.RestService.For<IClient>(c);
            // });

            //using Refit.HttpClientFactory
            //https://github.com/reactiveui/refit

            //var settings = new RefitSettings();
            //services.AddRefitClient<IClient, Client>(settings)
            //.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"));
        }

        //Transient
        public abstract void AddHttpClients(IServiceCollection services);
        #endregion

        #region gRPCClients
        //https://www.codemag.com/article/1807041/What%E2%80%99s-New-in-ASP.NET-Core-2.1
        public virtual void ConfiguregRPCClients(IServiceCollection services)
        {
            Logger.LogInformation("Configuring gRPC Clients");

            //Grpc.AspNetCore.Web, Grpc.Net.Client.Web
            services.AddGrpcWeb(options => { });

            //services.AddGrpcClient<IClient, Client>()
            //
            //    .AddHttpMessageHandler<ProfilingHttpHandler>()
            //    .AddHttpMessageHandler<AuthorizationBearerProxyHttpHandler>()
            //    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            //{
            //    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true,
            //    AllowAutoRedirect = true,
            //    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli
            //});

            //services.AddGrpcClient<>()

            //     .AddHttpMessageHandler<ProfilingHttpHandler>()
            //     .AddHttpMessageHandler<AuthorizationJwtProxyHttpHandler>()
            //    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            //    {
            //        ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true,
            //        AllowAutoRedirect = true,
            //        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli
            //    });

            AddgRPCClients(services);
        }

        public abstract void AddgRPCClients(IServiceCollection services);

        #endregion

        #region Profiling
        //https://miniprofiler.com/dotnet/AspDotNetCore
        public virtual void ConfigureProfilingServices(IServiceCollection services)
        {
            services.AddMiniProfiler("", false);
        }
        #endregion

        #region Identity
        //https://www.codemag.com/article/1807041/What%E2%80%99s-New-in-ASP.NET-Core-2.1
        public virtual void ConfigureIdentityServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Identity");

            services.AddUserService();

        }
        #endregion

        #region Hosted Services
        public virtual void ConfigureHostedServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Hosted Services");

            //Inject IBackgroundTaskQueue into Controller to trigger background tasks.
            //Queue.QueueBackgroundWorkItem(async token => {});
            services.AddHostedServiceBackgroundTaskQueue();

            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2
            AddHostedServices(services);

        }
        #endregion

        #region Notification Services
        public virtual void ConfigureNotificationServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Notification Services");

            services.AddSingleton<INotificationService, CompositeNotificationService>();
        }
        #endregion

        //https://github.com/RickStrahl/Westwind.AspnetCore.LiveReload
        #region Live Reload
        public virtual void ConfigureLiveReloadServices(IServiceCollection services)
        {
            Logger.LogInformation("Configuring Live Reload Services");
            services.AddLiveReload();
        }
        #endregion

        public abstract void AddDatabases(IServiceCollection services, ConnectionStrings connectionStrings, string identityConnectionString, string hangfireConnectionString, string defaultConnectionString);
        public abstract void AddRepositories(IServiceCollection services);
        public abstract void AddUnitOfWorks(IServiceCollection services);
        public abstract void AddApplicationServices(IServiceCollection services);
        public abstract void AddDomainServices(IServiceCollection services);
        public abstract void AddHostedServices(IServiceCollection services);
        public abstract void AddHangfireJobServices(IServiceCollection services);
        public abstract void AddStartupTasks(IServiceCollection services);
        public abstract void AddDbStartupTasks(IServiceCollection services);
        public abstract void AddHealthChecks(IHealthChecksBuilder healthCheckBuilder);

        //.NET Core 2.2
        //public abstract void AddGraphQLSchemas(IApplicationBuilder app);
        //app.UseGraphQL<MyHotelSchema>();

        public abstract void AddGraphQLSchemas(IEndpointRouteBuilder endpoints);
        //endpoints.MapGraphQL<MyHotelSchema>();
        #endregion

        #region 2. Configure Autofac Container
        public void ConfigureContainer(ContainerBuilder builder)
        {
            Logger.LogInformation("Configuring Autofac Modules");


        }
        #endregion

        #region 3. Configure Request Pipeline
        private static bool IsStreamRequest(Microsoft.AspNetCore.Http.HttpContext context)
        {
            var stream = false;

            var filename = Path.GetFileName(context.Request.Path.ToString());


            return stream;
        }

        private static bool AreCookiesConsentedCallback(Microsoft.AspNetCore.Http.HttpContext context, string cookieConsentName)
        {
            return context.Request.IsApi() || (context.Request.Cookies.Keys.Contains(cookieConsentName));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //In older tutorials, you may see similar code in the Configure method in Startup.cs. We recommend that you use the Configure method only to set up the request pipeline. Application startup code belongs in the Main method.
        //https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro?view=aspnetcore-2.2
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IServiceProvider serviceProvider, AppSettings appSettings, CacheSettings cacheSettings, AuthorizationSettings authorizationSettings,
            LocalizationSettings localizationSettings, SwitchSettings switchSettings, ServerSettings serverSettings, RequestLocalizationOptions localizationOptions, RedirectUnsupportedUrlCulturesOptions redirectUnsupportedUrlCulturesOptions,
            ISignalRHubMapper signalRHubMapper, ILoggerFactory loggerFactory, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            Logger.LogInformation("Configuring Request Pipeline");

            var cultureInfo = new CultureInfo(localizationSettings.DefaultCulture);

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            if (Options.DetectBlocking)
            {
                app.UseBlockingDetection();
            }

            //dotnet watch run
            // Before any other output generating middleware handlers
            //.NET Core 3.0
            if (HostingEnvironment.IsDevelopment())
            {
                app.UseLiveReload();
            }

            //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Core/src/Routing/UrlHelperFactory.cs
            //There seems to be a big differences between UrlHelper and EndpointRoutingUrlHelper

            if (HostingEnvironment.IsProduction())
            {
                app.UseRobotsTxt(builder =>
                builder
                .AddSection(section =>
                   section
                       .AddComment("Allow All")
                       .AddUserAgent("*")
                       .Allow("/")
                   )
                   .AddSitemap($"http://{appSettings.SiteUrl}/sitemap.xml")
               );
            }
            else
            {
                app.UseRobotsTxt(builder =>
                builder
                .AddSection(section =>
                  section
                      .AddComment("Disallow All")
                      .AddUserAgent("*")
                      .Disallow("/")
                  ));
            }

            var healthCheckOptions = new HealthCheckOptions()
            {
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                },
                ResponseWriter = HealthCheckJsonResponseWriter.WriteResponse,
                AllowCachingResponses = false
                //,Predicate = (check) => check.Tags.Contains("ready")
            };

            //.NET Core 2.2
            //app.UseHealthChecks("/hc", healthCheckOptions);

            if (!env.IsProduction())
            {
                app.UsePing("/ping");

                //Imortant: Must be before exception handling
                //1. download profiler from https://stackify.com/prefix/
                //2. enable .NET profiler in windows tray
                //3. access results at http: //localhost:2012
                app.UseStackifyPrefix();

                app.UseOutbound(appBranch =>
                {
                    appBranch.UseWhen(context => context.Request.IsMvc(), mvcBranch => mvcBranch.UseDeveloperExceptionPage());
                    appBranch.UseWhen(context => context.Request.IsApi(), apiBranch =>
                    {
                        apiBranch.UseProblemDetailsExceptionHandler(options => options.ShowExceptionDetails = true);
                        apiBranch.UseProblemDetailsErrorResponseHandler(options => options.HandleContentResponses = false);
                    });
                });

                app.UseDatabaseErrorPage();

                app.UseBrowserLink();

            }
            else
            {
                app.UseOutbound(appBranch =>
                {
                    appBranch.UseWhen(context => context.Request.IsMvc(), mvcBranch => mvcBranch.UseExceptionHandler("/Error"));
                    appBranch.UseWhen(context => context.Request.IsApi(), apiBranch =>
                    {
                        apiBranch.UseProblemDetailsExceptionHandler(options => options.ShowExceptionDetails = false);
                        apiBranch.UseProblemDetailsErrorResponseHandler(options => options.HandleContentResponses = false);
                    });
                });

                if (switchSettings.EnableHsts)
                {
                    //Only ever use HSTS in production!!!!!
                    //https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.1&tabs=visual-studio
                    //Strict-Transport-Security header is only added for HTTPS requests
                    //https://hstspreload.org/
                    //preload submits domain to chrome/safari so a users first request must be HTTPS.
                    app.UseHsts();
                }
            }


            if (switchSettings.EnableRedirectHttpToHttps)
            {
                //https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.1&tabs=visual-studio
                //picks up port automatically
                //Only works if there is a HTTPS binding.
                app.UseHttpsRedirection();
            }

            if (switchSettings.EnableRedirectNonWwwToWww)
            {
                var options = new RewriteOptions();
                options.AddRedirectToWww();
                //options.AddRedirectToHttps(StatusCodes.Status307TemporaryRedirect); // Does not pick up port automatically
                app.UseRewriter(options);
            }

            if (switchSettings.EnableHelloWorld)
            {
                app.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            }

            if (switchSettings.EnableSwagger)
            {
                app.UseSwaggerWithApiVersioning();
            }

            //Use Response Compression Middleware when you're:
            //Unable to use the following server-based compression technologies:
            //IIS Dynamic Compression module
            //http://www.talkingdotnet.com/how-to-enable-gzip-compression-in-asp-net-core/
            // General
            //"text/plain",
            // Static files
            //"text/css",
            //"application/javascript",
            // MVC
            //"text/html",
            //"application/xml",
            //"text/xml",
            //"application/json",
            //"text/json",
            if (switchSettings.EnableResponseCompression)
            {
                //https://www.softfluent.com/blog/dev/Enabling-gzip-compression-with-ASP-NET-Core
                //Concerning performance, the middleware is about 28% slower than the IIS compression (source). Additionally, IIS or nginx has a threshold for compression to avoid compressing very small files.
                app.UseResponseCompression();
            }

            //API rate limiting
            if (switchSettings.EnableIpRateLimiting)
            {
                app.UseIpRateLimiting();
            }

            if (switchSettings.EnableSignalR)
            {
                //.NET Core 2.2
                //app.UseSignalR(routes =>
                //{
                //    routes.MapHub<NotificationHub>(appSettings.SignalRUrlPrefix + "/signalr/notifications");
                //    signalRHubMapper.MapHubs(routes);
                //});
            }

            //--------------------------------------------- CACHING -----------------------------//

            //Cache-Control:max-age=0
            //This is equivalent to clicking Refresh, which means, give me the latest copy unless I already have the latest copy.
            //Cache-Control:no-cache 
            //This is holding Shift while clicking Refresh, which means, just redo everything no matter what.

            //Should only be used for server side HTML cachcing or Read Only API. Doesn't really make sense to use Response Caching for CRUD API. 

            //Will only attempt serve AND store caching if:
            //1. Controller or Action has ResponseCache attribute with Location = ResponseCacheLocation.Any
            //2. Request method is GET OR HEAD
            //3. AND Authorization header is not included

            //Will only attempt to serve from cache if:
            //1. Request header DOES NOT contain Cache-Control: no-cache (HTTP/1.1) AND Pragma: no-cache (HTTP/1.0)
            //2. AND Request header DOES NOT contain Cache-Control: max-age=0. Postman automatically has setting 'send no-cache header' switched on. This should be switched off to test caching.
            //3. AND Request header If-None-Match != Cached ETag
            //4. AND Request header If-Modified-Since < Cached Last Modified (Time it was stored in cache)

            //Will only attempt to store in cache if:
            //1. Request header DOES NOT contain Cache-Control: no-store (Added by RequestVerification)
            //2. AND Response header DOES NOT contain Cache-Control: no-store
            //3. AND Response header does not contain Set-Cookie
            //4. AND Response Status is 200

            //When storing
            //1. Stores all headers except Age
            //2. Stores Body
            //3. Stores Length

            //In memory cache
            //https://www.devtrends.co.uk/blog/a-guide-to-caching-in-asp.net-core
            //Unfortunately, the built-in response caching middleware makes this very difficult. 
            //Firstly, the same cache duration is used for both client and server caches. Secondly, currently there is no easy way to invalidate cache entries.
            //app.UseResponseCaching();
            //Request Header Cache-Control: max-age=0 or no-cache will bypass Response Caching. Postman automatically has setting 'send no-cache header' switched on. This should be switched off to test caching.
            if (switchSettings.EnableResponseCaching)
            {
                if (switchSettings.EnableCookieConsent)
                {
                    app.UseWhen(context => AreCookiesConsentedCallback(context, appSettings.CookieConsentName) 
                    && !IsStreamRequest(context),
                      appBranch =>
                      {
                          appBranch.UseResponseCachingCustom(); //Allows Invalidation
                      }
                    );
                }
                else
                {
                    app.UseWhen(context => !IsStreamRequest(context),
                       appBranch =>
                       {
                           appBranch.UseResponseCachingCustom(); //Allows Invalidation
                       }
                     );
                }
            }

            //Works for: GET, HEAD (efficiency, and saves bandwidth)
            //Works for: PUT, PATCH (Concurrency)s
            //This is Etags
            //Generating ETags is expensive. Putting this after response caching makes sense.
            if (switchSettings.EnableETags)
            {
                if (switchSettings.EnableCookieConsent)
                {
                    app.UseWhen(context => AreCookiesConsentedCallback(context, appSettings.CookieConsentName) 
                    && !IsStreamRequest(context),
                      appBranch =>
                      {
                          appBranch.UseHttpCacheHeaders();
                      }
                    );
                }
                else
                {
                    app.UseWhen(context => !IsStreamRequest(context),
                     appBranch =>
                     {
                         appBranch.UseHttpCacheHeaders();
                     }
                   );
                }
            }

            app.UseMiniProfiler();

            //--------------------------------------------- REQUEST LOCALIZATION -----------------------------//

            app.UseRequestLocalization(localizationOptions);
            app.UseRedirectUnsupportedUrlCultures(redirectUnsupportedUrlCulturesOptions);
            app.UseUrlUnlocalization();

            //--------------------------------------------- SECURITY HEADERS -----------------------------//

            var policyCollection = new HeaderPolicyCollection();
            ConfigureSecurityHeaders(policyCollection);

            app.UseSecurityHeaders(policyCollection);

            //--------------------------------------------- STATIC FILES -----------------------------//

            app.MapWhen(
               context => context.Request.Path.ToString().StartsWith(AssetsFolder),
               appBranch =>
               {
                   // ... optionally add more middleware to this branch
                   char[] seperator = { ',' };
                   List<string> publicUploadFolders = appSettings.PublicUploadFolders.Split(seperator).ToList();
                   appBranch.UseContentHandler(env, options =>
                   {
                       options.CacheDays = cacheSettings.UploadFilesDays;
                       options.ValidFolders = publicUploadFolders;
                       options.FFMpeg = AppSettings.FFMpeg;
                       options.ImageWatermarkEnabled = AppSettings.ImageWatermarkEnabled;
                       options.ImageWatermark = AppSettings.ImageWatermark;
                       options.ImageWatermarkMinHeight = AppSettings.ImageWatermarkMinHeight;
                       options.ImageWatermarkMinWidth = AppSettings.ImageWatermarkMinWidth;
                   });
               });

            app.UseDefaultFiles();

            app.UseMultitenantStaticFiles();

            //versioned files can have large cache expiry time
            app.UseVersionedStaticFiles(cacheSettings.VersionedStaticFilesDays);

            //non versioned files
            app.UseNonVersionedStaticFiles(cacheSettings.NonVersionedStaticFilesDays);

            //ClientSide Blazor
            if (Options.BlazorWASMApp)
            {
                //app.UseBlazorFrameworkFiles();
            }

            //Spa static files
            if (Options.ReactApp || Options.AngularApp)
            {
                app.UseSpaGenerateAntiforgeryTokenMiddleware();

                if (!env.IsDevelopment())
                {
                    // In production the React/Angular files will be served from ClientApp/build or ClientApp/dist
                    app.UseSpaStaticFiles();
                }
            }

            //--------------------------------------------- COOKIES -----------------------------//

            if (switchSettings.EnableCookieConsent)
            {
                app.UseCookiePolicy();
            }

            //Need to use SPAXSRFTokenMiddleware if storing JWT in cookies!
            app.UseJwtCookieAuthentication();

            //-------------------------------------------- LOGGING -----------------------------//
            //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-reducing-log-verbosity/
            //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-the-selected-endpoint-name-with-serilog/
            //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-logging-mvc-propertis-with-serilog/
            //https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-excluding-health-check-endpoints-from-serilog-request-logging/
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = LoggingAspNetCore.EnrichFromRequest;
                options.GetLevel = LoggingAspNetCore.ExcludeHealthChecks; // Use the custom level
            });

            //--------------------------------------------- ROUTING - endpoint resolution middlware -----------------------------//
            //.NET Core 3.0
            // 1. Runs matching. An endpoint is selected and set on the HttpContext if a match is found.
            app.UseRouting();

            //resolves the route and adds the Endpoint object to the httpcontext
            //.NET Core 2.2 the MVC middleware at the end of the pipeline acts as the endpoint dispatcher middleware. It will dispatch the resolved endpoint to the proper controller action.
            //app.UseEndpointRouting();

            //CORS is only for AJAX requests
            //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.0
            //With endpoint routing, the CORS middleware must be configured to execute between the calls to UseRouting and UseEndpoints. Incorrect configuration will cause the middleware to stop functioning correctly.
            if (switchSettings.EnableCors)
            {
                if (HostingEnvironment.IsProduction())
                {
                    app.UseCors("AllowSpecificOrigin");
                }
                else
                {
                    app.UseCors("AllowAnyOrigin");
                }
            }

            // 2. Middleware that run after routing occurs.
            //.NET Core 3.0
            app.UseCertificateForwarding();

            app.UseAuthentication();

            //.NET Core 3.0
            app.UseAuthorization();
            app.UseGrpcWeb();

            //Matched Route
            app.Use((context, next) =>
            {
                var endpointFeature = context.Features[typeof(IEndpointFeature)] as IEndpointFeature;
                var endpoint = endpointFeature?.Endpoint;

                //note: endpoint will be null, if there was no
                //route match found for the request by the endpoint route resolver middleware
                if (endpoint != null)
                {
                    var routePattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText;

                    //Console.WriteLine("Name: " + endpoint.DisplayName);
                    //Console.WriteLine($"Route Pattern: {routePattern}");
                    //Console.WriteLine("Metadata Types: " + string.Join(", ", endpoint.Metadata));
                }
                else
                {
                    //Console.WriteLine("No route found");
                }
                return next();
            });


            //.NET Core 2.2
            //if (switchSettings.EnableHangfire)
            //{
            //    if (Configuration.GetValue<bool>("MultiTenant", false))
            //        app.UseHangfireDashboardMultiTenant();
            //    else
            //        app.UseHangfireDashboard("/admin/hangfire");
            //}

            //app.UseWebSockets(); //required for GraphQL

            //AddGraphQLSchemas(app);

            //app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());

            //var routeBuilder = new RouteBuilder(app);

            //app.UseRouter(routeBuilder.Build());

            ////.NET Core 2.2
            //app.UseMvc(routes =>
            //{
            //    routes.MapAllRoutes("/routes");

            //    //https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-2.2
            //    if (redirectUnsupportedUrlCulturesOptions.RedirectCulturelessToDefaultCulture)
            //    {
            //        routes.RedirectCulturelessToDefaultCulture();
            //    }
            //});

            //.NET Core 3.0
            //https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-core-3-0-preview-4/
            // 3. Executes the endpoint that was selected by routing. replaces UseMvc and UseRouter

            //--Route template precedence and endpoint selection order--
            //Route template precedence is a system that assigns each route template a value based on how specific it is.Route template precedence:

            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-3.1
            //http://rion.io/2015/11/13/understanding-routing-precedence-in-asp-net-mvc/
            //Avoids the need to adjust the order of endpoints in common cases.
            //Attempts to match the common - sense expectations of routing behavior.
            //For example, consider templates /Products/List and /Products/{id}. It would be reasonable to assume that /Products/List is a
            //better match than /Products/{id} for the URL path /Products/List. The works because the literal segment /List is considered to 
            //have better precedence than the parameter segment /{id}.

            //The details of how precedence works are coupled to how route templates are defined:

            //Templates with more segments are considered more specific.
            //A segment with literal text is considered more specific than a parameter segment.
            //A parameter segment with a constraint is considered more specific than one without.
            //A complex segment is considered as specific as a parameter segment with a constraint.
            //Catch all parameters are the least specific.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", healthCheckOptions).RequireAuthorization("HealthCheckPolicy");
                //.RequireCors(builder =>
                //{
                //    builder.AllowAnyOrigin(); //Browser
                //});

                endpoints.MapAllRoutes("/routes");

                if (switchSettings.EnableHangfire)
                {
                    if (Configuration.GetValue<bool>("MultiTenant", false))
                        endpoints.MapHangfireMultiTenantDashboard();
                    else
                        endpoints.MapHangfireDashboard("/admin/hangfire");
                }

                AddGraphQLSchemas(endpoints);
                endpoints.MapGraphQLPlayground();
                endpoints.MapGraphQLVoyager();

                var builder = endpoints.MapControllers();
                //if (authorizationSettings.UserMustBeAuthorizedByDefault)
                //builder.RequireAuthorization();
                endpoints.MapRazorPages();

                endpoints.MapBlazorHub(); //Blazor SignalR Serverside connections for Components

                if (Options.BlazorServerApp)
                {
                    //endpoints.MapFallbackToPage("/_Host"); //Blazor ServerSide App 
                }
                else if (Options.BlazorWASMApp)
                {
                    //endpoints.MapFallbackToFile("index.html"); //Blazor ClientSide App
                }

                if (switchSettings.EnableSignalR)
                {
                    endpoints.MapHub<NotificationHub>(appSettings.SignalRUrlPrefix + "/signalr/notifications");
                    signalRHubMapper.MapHubs(endpoints);
                }

                //if (redirectUnsupportedUrlCulturesOptions.RedirectCulturelessToDefaultCulture)
                    //endpoints.RedirectCulturelessToDefaultCulture();

                //https://devblogs.microsoft.com/aspnet/grpc-web-experiment/
                //var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
                //var channel = Grpc.Channel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpClient = new HttpClient(handler)});
                //var client = Greeter.GreeterClient(channel);

                //endpoints.MapGrpcService<MyCalculatorService>().EnableGrpcWeb(); //Grpc.Net.Client.Web
            });

            var routeBuilder = new RouteBuilder(app);
            if (redirectUnsupportedUrlCulturesOptions.RedirectCulturelessToDefaultCulture)
                routeBuilder.RedirectCulturelessToDefaultCulture();
            app.UseRouter(routeBuilder.Build());

            // 4. Middleware here will only run if nothing was matched.

            if (Options.ReactApp)
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                });
            }
            else if (Options.AngularApp)
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
            }
        }

        //https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
        //web.config for remvoving IIS headers
        public virtual void ConfigureSecurityHeaders(HeaderPolicyCollection policyCollection)
        {
            policyCollection
            .AddFrameOptionsSameOrigin() //X-Frame-Options: SAMEORIGIN
            .AddXssProtectionBlock() //X-XSS-Protection 1; mode=block
            .AddContentTypeOptionsNoSniff() //X-Content-Type-Options: nosniff
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .RemoveServerHeader()
            .AddContentSecurityPolicy(builder =>
            {
                //builder.AddCustomDirective("require-sri-for", "script style"); //integrity https://www.srihash.org/
                builder.AddObjectSrc().None();
                builder.AddFormAction().Self();
                builder.AddFrameAncestors().Self();
            });
        }
        #endregion
    }
}
