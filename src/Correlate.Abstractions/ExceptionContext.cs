using System;
using System.Diagnostics;

namespace Correlate
{
	/// <summary>
	/// Represents a context that provides access to the exception that occurred inside a correlated activity, with the ability to mark the exception as handled.
	/// </summary>
	public class ExceptionContext
	{
		internal ExceptionContext()
		{
		}

		/// <summary>
		/// Gets the correlation context
		/// </summary>
		public CorrelationContext CorrelationContext { get; internal set; }

		/// <summary>
		/// Gets the exception that occurred.
		/// </summary>
		public Exception Exception { get; internal set; }

		/// <summary>
		/// Gets or sets whether the exception is considered handled.
		/// </summary>
		public bool IsExceptionHandled { get; set; }
	}

	/// <summary>
	/// Represents a context that provides access to the exception that occurred inside a correlated activity, with the ability to mark the exception as handled and provide a return value.
	/// </summary>
	public class ExceptionContext<T> : ExceptionContext
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private T _result;

		/// <summary>
		/// Gets or sets the result value to return. This is only 
		/// </summary>
		public T Result
		{
			get
			{
				return _result;
			}
			set
			{
				IsExceptionHandled = true;
				_result = value;
			}
		}
	}
}