using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging.Abstractions
{
#if !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NET5_0
	/// <summary>
	/// Polyfill for frameworks older than NET Core 2.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NullLogger<T> : ILogger<T>
	{
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			NullLogger.Instance.Log(logLevel, eventId, state, exception, formatter);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return NullLogger.Instance.IsEnabled(logLevel);
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return NullLogger.Instance.BeginScope(state);
		}
	}
#endif
}
