namespace Correlate;

/// <summary>
/// A delegate for handling errors inside correlation scope.
/// </summary>
/// <param name="errorContext">The error context.</param>
#pragma warning disable CA1711
public delegate void OnError(ErrorContext errorContext);

/// <summary>
/// A delegate for handling errors inside correlation scope.
/// </summary>
/// <param name="errorContext">The error context.</param>
public delegate void OnError<T>(ErrorContext<T> errorContext);
#pragma warning restore CA1711
