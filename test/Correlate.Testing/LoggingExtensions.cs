using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.Testing
{
	public static class LoggingExtensions
	{
		public static IServiceCollection ForceEnableLogging(this IServiceCollection services)
		{
			return services.AddLogging(logging => logging.AddProvider(new TestLoggerProvider()));
		}

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