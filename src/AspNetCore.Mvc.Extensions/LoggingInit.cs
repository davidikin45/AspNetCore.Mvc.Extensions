using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Logging
{
    public class LoggingInit
    {
        //Serilog
        //Serilog.AspNetCore - UsingSerilog()
        //Serilog.Enrichers.Environment
        //Serilog.Exceptions
        //Serilog.Settings.Configuration
        //Serilog.Sinks.Console
        //Serilog.Sinks.Debug
        //Serilog.Sinks.Elasticsearch
        //Serilog.Sinks.File
        //Serilog.Sinks.Seq

        //Serilog.Formatting.Compact
        //Cant have formatter and outputTemplate - https://stackoverflow.com/questions/48646420/serilog-jsonformatter-in-an-asp-net-core-2-not-being-applied-from-appsettings-fi
        //"formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        //"formatter": "Serilog.Formatting.Compact.RenderedCompactJsonFormatter, Serilog.Formatting.Compact"
        //"formatter": "Serilog.Formatting.Elasticsearch.ElasticsearchJsonFormatter, Serilog.Formatting.Elasticsearch"

        //Category and Events are logical grouping
        //Easy to see all log entries of a certain type of event.
        //public static EventId GetMany = new EventId(10001, "GetManyFromProc")

        //Scopes - shared content included in each log entry even if in another assembly.
        //using(_logger.BeginScope("Starting operation for {UserId}", userId))

        //High Performance Logging
        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-2.2

        //http://mygeekjourney.com/asp-net-core/integrating-serilog-asp-net-core/
        //https://www.carlrippon.com/asp-net-core-logging-with-serilog-and-sql-server/
        //https://www.humankode.com/asp-net-core/logging-with-elasticsearch-kibana-asp-net-core-and-docker
        /// <summary>
        /// Trace = 0
        /// Debug = 1 -- Developement Standard
        /// Information = 2 -- LogFactory Default
        /// Warning = 3 -- Production Standard
        /// Error = 4 -- Generally Unhandled Exception
        /// Critical = 5 -- Complete Application Fail, DB unavailable
        /// </summary>
        public static void Init(IConfiguration configuration, string elasticUri = null, string seqUri = null)
        {
            var name = AssemblyHelper.GetEntryAssembly().GetName();
            var loggerConfiguration = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration)
             .Enrich.FromLogContext() //using (LogContext.PushProperty("A", 1))
             .Enrich.WithExceptionDetails() //Include exception.data for saving exception extra data.
             .Enrich.WithMachineName() //Environment.MachineName
             .Enrich.WithProperty("Assembly", $"{name.Name}")
             .Enrich.WithProperty("Version", $"{name.Version}")
             .AddElasticSearchLogging(elasticUri)
             .AddElasticSearchLogging(configuration)
             .AddSeqLogging(seqUri)
             .AddSeqLogging(configuration);

            //Serilog will use Log.Logger by default.
            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
