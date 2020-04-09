using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace AspNetCore.Mvc.Extensions.Logging
{
    public static class ActionLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddAction(this ILoggingBuilder builder, Action<string> logAction)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new ActionLoggerProvider(logAction));
            return builder;
        }
    }

    [ProviderAlias("Action")]
    public class ActionLoggerProvider : ILoggerProvider
    {
        private readonly Action<string> _logAction;

        public ActionLoggerProvider(Action<string> logAction)
        {
            _logAction = logAction;
        }

        public ILogger CreateLogger(string name)
        {
            return new ActionLogger(name, _logAction);
        }

        public void Dispose()
        {
        }
    }
    public partial class ActionLogger : ILogger
    {
        private readonly Action<string> _logAction;
        private readonly string _name;

        public ActionLogger(string name, Action<string> logAction) : this(name, filter: null, logAction: logAction)
        {
        }

        public ActionLogger(string name, Func<string, LogLevel, bool> filter, Action<string> logAction)
        {
            _name = string.IsNullOrEmpty(name) ? nameof(ActionLogger) : name;
            _logAction = logAction;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logAction != null;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            message = $"{ logLevel }: {message}";

            if (exception != null)
            {
                message += Environment.NewLine + Environment.NewLine + exception.ToString();
            }

            _logAction(message);
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
