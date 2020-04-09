using AspNetCore.Mvc.Extensions;
using AspNetCore.Mvc.Extensions.Cqrs;
using AspNetCore.Mvc.Extensions.DomainEvents;
using AspNetCore.Mvc.Extensions.IntegrationEvents;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.StartupTasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Commands;
using TemplateAspNetCore3.Data;
using TemplateAspNetCore3.DomainEvents;
using TemplateAspNetCore3.Models;
using TemplateAspNetCore3.Queries;

namespace TemplateAspNetCore3
{
    public class Startup : AppStartupMvcIdentity<IdentityContext, User>
    {

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
            : base(configuration, hostingEnvironment, options => {
                options.ScanAssembliesForDbStartupTasks = true;
                options.ScanAssembliesForStartupTasks = true;
            })
        {
            
        }

        public override void AddDatabases(IServiceCollection services, ConnectionStrings connectionStrings, string tenantsConnectionString, string identityConnectionString, string hangfireConnectionString, string defaultConnectionString)
        {
            services.AddDbContextNoSql<NoSqlContext>(connectionStrings["NoSqlConnection"]);
            services.AddDbContext<AppContext>(defaultConnectionString);
            services.AddDbContext<IdentityContext>(identityConnectionString);
        }

        public override void AddUnitOfWorks(IServiceCollection services)
        {
            services.AddUnitOfWork<IAppUnitOfWork, AppUnitOfWork>();
        }

        public override void AddHostedServices(IServiceCollection services)
        {
            //services.AddHostedServiceCronJob<Job2>("* * * * *");
        }

        public override void AddHangfireJobServices(IServiceCollection services)
        {

            //services.AddHangfireJob<Job1>();
        }

        public override void AddHttpClients(IServiceCollection services)
        {

        }

        public override void AddgRPCClients(IServiceCollection services)
        {

        }

        public override void AddGraphQLSchemas(IEndpointRouteBuilder endpoints)
        {
            
        }

        public override void AddDbStartupTasks(IServiceCollection services)
        {

        }

        public override void AddStartupTasks(IServiceCollection services)
        {

        }
    }

    public class HangfireScheduledJobs : IStartupTask
    {
        private readonly IRecurringJobManager _recurringJobManager;
        public HangfireScheduledJobs(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public int Order => 0;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_recurringJobManager.AddOrUpdate("check-link", Job.FromExpression<Job1>(m => m.Execute()), Cron.Minutely(), new RecurringJobOptions());
            //_recurringJobManager.Trigger("check-link");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class CqrsCommandsTask : IStartupTask
    {
        private readonly ICqrsMediator _mediator;

        public int Order => 0;
        public CqrsCommandsTask(ICqrsMediator mediator)
        {
            _mediator = mediator;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _mediator.CqrsCommandSubscriptionManager.AddDynamicSubscription<dynamic, object, CommandHandler>("*");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class CqrsQueriesTask : IStartupTask
    {
        private readonly ICqrsMediator _mediator;

        public int Order => 0;
        public CqrsQueriesTask(ICqrsMediator mediator)
        {
            _mediator = mediator;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _mediator.CqrsQuerySubscriptionManager.AddDynamicSubscription<dynamic, object, QueryHandler>("*");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class DomainEventsTask : IStartupTask
    {
        private readonly IDomainEventBus _eventBus;

        public int Order => 0;
        public DomainEventsTask(IDomainEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _eventBus.DomainEventSubscriptionsManager.AddDynamicSubscription<dynamic, DomainEventHandler>("*");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class IntegrationEventsTask : IStartupTask
    {
        private readonly IIntegrationEventBus _eventBus;

        public int Order => 0;
        public IntegrationEventsTask(IIntegrationEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
