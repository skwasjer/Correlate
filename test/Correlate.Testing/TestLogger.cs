using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Correlate.Testing;

public sealed class TestLogger<T> : TestLogger, ILogger<T>
{
    public TestLogger(ILogger? innerLogger, bool isEnabled = true)
        : base(innerLogger, typeof(T).FullName!, isEnabled)
    {
    }

    public TestLogger(bool isEnabled = true)
        : this(null, isEnabled)
    {
    }
}

public class TestLogger : ILogger
{
    private readonly ILogger? _innerLogger;
    private readonly bool _isEnabled;

    // ReSharper disable once UnusedParameter.Local
#pragma warning disable IDE0060
    protected TestLogger(ILogger? innerLogger, string name, bool isEnabled = true)
#pragma warning restore IDE0060
    {
        _innerLogger = innerLogger;
        _isEnabled = isEnabled;
    }

    public TestLogger(string name, bool isEnabled = true)
        : this(null, name, isEnabled)
    {
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _innerLogger?.Log(logLevel, eventId, state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _isEnabled;
    }

#if NET8_0_OR_GREATER
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
#else
    public IDisposable BeginScope<TState>(TState state)
#endif
    {
        return _innerLogger?.BeginScope(state) ?? NullLogger.Instance.BeginScope(state);
    }
}
