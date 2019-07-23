namespace Correlate
{
	/// <summary>
	/// A delegate for handling exception inside correlation scope.
	/// </summary>
	/// <param name="exceptionContext">The exception context.</param>
	public delegate void OnException(ExceptionContext exceptionContext);

	/// <summary>
	/// A delegate for handling exception inside correlation scope.
	/// </summary>
	/// <param name="exceptionContext">The exception context.</param>
	public delegate void OnException<T>(ExceptionContext<T> exceptionContext);
}