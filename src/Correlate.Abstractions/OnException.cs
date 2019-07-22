using System;

namespace Correlate
{
	/// <summary>
	/// A delegate for handling exception inside correlation scope.
	/// </summary>
	/// <param name="correlationContext">The correlation context.</param>
	/// <param name="exception">The exception.</param>
	/// <returns><see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</returns>
	public delegate bool OnException(CorrelationContext correlationContext, Exception exception);
}