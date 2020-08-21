using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Data.Migrations;
using AspNetCore.Mvc.Extensions.Logging;
using AspNetCore.Specification.Data;
using Autofac.AspNetCore.Extensions;
using Autofac.AspNetCore.Extensions.Data;
using EntityFrameworkCore.Initialization.Converters;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data
{
    public abstract class DbContextBase : DbContextTenantBase, IQueriesDbContext
    {
        protected DbContextBase(ITenantService tenantService)
            : base(tenantService)
        {
        }

        public DbContextBase(DbContextOptions options, ITenantService tenantService)
            : base(options, tenantService)
        {

        }

        public bool LazyLoadingEnabled
        {
            get { return ChangeTracker.LazyLoadingEnabled; }
            set { ChangeTracker.LazyLoadingEnabled = value; }
        }

        //Scans tracked entities. 
        //For Add its important for child entities
        //For Update it detects changes without needing to call Context.Update
        public bool AutoDetectChangesEnabled
        {
            get { return ChangeTracker.AutoDetectChangesEnabled; }
            set { ChangeTracker.AutoDetectChangesEnabled = value; }
        }

        //Finds are always tracked.
        //Only reveleant to IQueryable
        public QueryTrackingBehavior DefaultQueryTrackingBehavior
        {
            get { return ChangeTracker.QueryTrackingBehavior; }
            set { ChangeTracker.QueryTrackingBehavior = value; }
        }

        public static readonly ILoggerFactory CommandLoggerFactory
         = new ServiceCollection().AddLogging(builder =>
         {
             builder.AddDebug().AddConsole().AddAction(LogCommand).AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
         }).BuildServiceProvider()
         .GetService<ILoggerFactory>();

        public static readonly ILoggerFactory ChangeTrackerLoggerFactory
         = new ServiceCollection().AddLogging(builder =>
         {
             builder.AddDebug().AddConsole().AddAction(LogCommand).AddFilter(DbLoggerCategory.ChangeTracking.Name, LogLevel.Debug);
         }).BuildServiceProvider()
         .GetService<ILoggerFactory>();

        public static Action<string> Log { get; set; }
        private static void LogCommand(string log)
        {
            Log?.Invoke(log);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

#pragma warning disable CS0618 // Type or member is obsolete
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
#pragma warning restore CS0618 // Type or member is obsolete

            if (optionsBuilder.Options.Extensions is CoreOptionsExtension coreOptionsExtension)
            {
                if (coreOptionsExtension.LoggerFactory == null)
                {
                    //Output commands to console window
                    optionsBuilder.UseLoggerFactory(CommandLoggerFactory);
                }
            }

            //Enable Parameter values otherwise ?
            optionsBuilder.EnableSensitiveDataLogging();

            optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, CompositeMigrationsAnnotationsProvider>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            builder.UseIdentityColumns();

            //.NET Core 2.2 
            //builder.ForSqlServerUseIdentityColumns();

            builder.RemovePluralizingTableNameConvention();
            builder.AddSoftDeleteFilter();

            //Add Seed Data for things like Enumerations > Lookup Tables. Migrations are generated for this data.

            builder.AddEncryptedValues("password");
            builder.AddJsonValues();
            builder.AddCsvValues();
            builder.AddMultiLangaugeStringValues();
            builder.AddBackingFields();
      
            AddKeylessEntities(builder);
        }

        //[Keyless] EF Core 5.0
        //https://docs.microsoft.com/en-us/ef/core/modeling/keyless-entity-types?tabs=data-annotations
        public abstract void AddKeylessEntities(ModelBuilder builder);

        #region MSI Access Token
        //https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi
        public string GetMSIAccessToken()
        {
            var accessToken = (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();
            return accessToken;
        }
        #endregion

        #region Migrate
        public void Migrate()
        {
            Database.Migrate();
        }
        #endregion

        #region Save Changes
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetTimestamps();
            var auditLogs = this.AuditBeforeSaveChanges();
      
            var changes = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            await this.AuditAfterSaveChangesAsync(auditLogs, cancellationToken);
            return changes;
        }

        public override int SaveChanges()
        {

            this.SetTimestamps();
            var auditLogs = this.AuditBeforeSaveChanges();

            var changes = base.SaveChanges();

            this.AuditAfterSaveChanges(auditLogs);
            return changes;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetTimestamps();
            var auditLogs = this.AuditBeforeSaveChanges();

            var changes = base.SaveChanges(acceptAllChangesOnSuccess);

            this.AuditAfterSaveChanges(auditLogs);
            return changes;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetTimestamps();
            var auditLogs = this.AuditBeforeSaveChanges();

            var changes = await base.SaveChangesAsync(cancellationToken);

            await this.AuditAfterSaveChangesAsync(auditLogs, cancellationToken);
            return changes;
        }
        #endregion

        #region Specification Query
        public SpecificationDbQuery<TEntity> SpecificationQuery<TEntity>() where TEntity : class
        {
            return new SpecificationDbQuery<TEntity>(this);
        }
        #endregion
    }
}
