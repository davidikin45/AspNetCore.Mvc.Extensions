using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Data.Migrations;
using AspNetCore.Specification.Data;
using Autofac.AspNetCore.Extensions;
using Autofac.AspNetCore.Extensions.Data;
using EntityFrameworkCore.Initialization.Converters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data
{
    public abstract class DbContextIdentityBase<TUser> : DbContextIdentityTenantBase<TUser>, IQueriesDbContext where TUser : IdentityUser
    {
        protected DbContextIdentityBase(ITenantService tenantService)
            :base(tenantService)
        {

        }

        public DbContextIdentityBase(DbContextOptions options, ITenantService tenantService)
            : base(options, tenantService)
        {

        }

        public bool LazyLoadingEnabled
        {
            get { return ChangeTracker.LazyLoadingEnabled; }
            set { ChangeTracker.LazyLoadingEnabled = value; }
        }

        public bool AutoDetectChangesEnabled
        {
            get { return ChangeTracker.AutoDetectChangesEnabled; }
            set { ChangeTracker.AutoDetectChangesEnabled = value; }
        }

        public QueryTrackingBehavior DefaultQueryTrackingBehavior
        {
            get { return ChangeTracker.QueryTrackingBehavior; }
            set { ChangeTracker.QueryTrackingBehavior = value; }
        }

        public static readonly ILoggerFactory CommandLoggerFactory
        = new ServiceCollection().AddLogging(builder =>
        {
            builder.AddDebug().AddConsole().AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
        }).BuildServiceProvider()
        .GetService<ILoggerFactory>();

        public static readonly ILoggerFactory ChangeTrackerLoggerFactory
         = new ServiceCollection().AddLogging(builder =>
         {
             builder.AddDebug().AddConsole().AddFilter(DbLoggerCategory.ChangeTracking.Name, LogLevel.Debug);
         }).BuildServiceProvider()
         .GetService<ILoggerFactory>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

#pragma warning disable CS0618 // Type or member is obsolete
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
#pragma warning restore CS0618 // Type or member is obsolete

            if (optionsBuilder.Options.Extensions is CoreOptionsExtension)
            {
                var coreOptionsExtension = optionsBuilder.Options.Extensions as CoreOptionsExtension;
                if (coreOptionsExtension.LoggerFactory == null)
                {
                    optionsBuilder.UseLoggerFactory(CommandLoggerFactory);
                }
            }
            optionsBuilder.EnableSensitiveDataLogging();

            optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, CompositeMigrationsAnnotationsProvider>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //builder.HasDefaultSchema("dbo"); //SQLite doesnt have schemas

            builder.UseIdentityColumns();

            //.NET Core 2.2 
            //builder.ForSqlServerUseIdentityColumns();

            builder.RemovePluralizingTableNameConvention();
            builder.AddSoftDeleteFilter();

            builder.AddEncryptedValues("password");
            builder.AddJsonValues();
            builder.AddCsvValues();
            builder.AddMultiLangaugeStringValues();
            builder.AddBackingFields();

            //modelBuilder.Entity<IdentityUser>().ToTable("User");
            builder.Entity<TUser>().ToTable("User");
            builder.Entity<IdentityRole>().ToTable("Role");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");

            AddKeylessEntities(builder);
        }

        //[Keyless] EF Core 5.0
        //https://docs.microsoft.com/en-us/ef/core/modeling/keyless-entity-types?tabs=data-annotations
        public abstract void AddKeylessEntities(ModelBuilder builder);

        #region Migrate
        public void Migrate()
        {
            Database.Migrate();
        }
        #endregion

        #region Save Changes
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken
            = default(CancellationToken))
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
