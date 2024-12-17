using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Correlate;

/// <summary>
/// Represents a context that provides access to the exception that occurred inside a correlated activity, with the ability to mark the error as handled.
/// </summary>
public class ErrorContext
{
    internal ErrorContext(CorrelationContext correlationContext, Exception exception)
    {
        CorrelationContext = correlationContext;
        Exception = exception;
    }

    /// <summary>
    /// Gets the correlation context
    /// </summary>
    public CorrelationContext CorrelationContext { get; }

    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets or sets whether the error is considered handled.
    /// </summary>
    public bool IsErrorHandled { get; set; }
}

/// <summary>
/// Represents a context that provides access to the exception that occurred inside a correlated activity, with the ability to mark the error as handled and provide a return value.
/// </summary>
public class ErrorContext<T> : ErrorContext
{
    // ReSharper disable once RedundantDefaultMemberInitializer
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private T _result = default!;

    internal ErrorContext(CorrelationContext correlationContext, Exception exception)
        : base(correlationContext, exception)
    {
    }

    /// <summary>
    /// Gets or sets the result value to return.
    /// </summary>
    [AllowNull]
    public T Result
    {
        get => _result;
        set
        {
            IsErrorHandled = true;
            _result = value!;
        }
    }
}
