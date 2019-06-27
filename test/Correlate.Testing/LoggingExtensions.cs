using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.Testing
{
	public static class LoggingExtensions
	{
#if NETSTANDARD1_3
		public static IServiceCollection ForceEnableLogging(this IServiceCollection services)
		{
			return services
				.AddLogging()
				.AddLoggingProvider(new TestLoggerProvider());
		}

		private static IServiceCollection AddLoggingProvider(this IServiceCollection services, ILoggerProvider loggerProvider)
		{
			return services.AddSingleton(loggerProvider);
		}
#else
		public static IServiceCollection ForceEnableLogging(this IServiceCollection services)
		{
			return services.AddLogging(logging => logging.AddProvider(new TestLoggerProvider()));
		}
#endif

		private class TestLoggerProvider : ILoggerProvider
		{
			private TestLogger _testLogger;

			public void Dispose()
			{
			}

			public ILogger CreateLogger(string categoryName)
			{
				return _testLogger ?? (_testLogger = new TestLogger());
			}
		}
	}
}