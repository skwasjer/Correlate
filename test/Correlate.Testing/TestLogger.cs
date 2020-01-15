using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Correlate.Testing
{
	public class TestLogger<T> : TestLogger, ILogger<T>
	{
		public TestLogger(ILogger innerLogger, bool isEnabled = true)
			: base(innerLogger, typeof(T).FullName, isEnabled)
		{
		}

		public TestLogger(bool isEnabled = true)
			: this(null, isEnabled)
		{
		}
	}

	public class TestLogger : ILogger
	{
		private readonly bool _isEnabled;
		private readonly ILogger _innerLogger;

		public TestLogger(ILogger innerLogger, string name, bool isEnabled = true)
		{
			_innerLogger = innerLogger;
			_isEnabled = isEnabled;
		}

		public TestLogger(string name, bool isEnabled = true)
			: this(null, name, isEnabled)
		{
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_innerLogger?.Log(logLevel, eventId, state, exception, formatter);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _isEnabled;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return _innerLogger?.BeginScope(state) ?? NullLogger.Instance.BeginScope(state);
		}
	}
}
