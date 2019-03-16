using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Correlate.Testing
{
	public static class LoggingExtensions
	{
		public static IServiceCollection ForceEnableLogging(this IServiceCollection services)
		{
			return services.AddLogging(logging => logging.AddProvider(new DummyProvider()));
		}

		private class DummyProvider : ILoggerProvider
		{
			private DummyLogger _dummyLogger;

			public void Dispose()
			{
			}

			public ILogger CreateLogger(string categoryName)
			{
				return _dummyLogger ?? (_dummyLogger = new DummyLogger());
			}
		}

		private class DummyLogger : ILogger
		{
			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return true;
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				return NullScope.Instance;
			}
		}
	}
}