using System.ComponentModel;
using System.Diagnostics;

namespace Correlate
{
	public class CorrelateOptions
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal const string DefaultHeaderName = "X-Correlation-ID";

		/// <summary>
		/// Gets or sets the request headers to retrieve the correlation id from.
		/// </summary>
		/// <remarks>
		/// The first matching header will be used.
		/// </remarks>
		public string[] RequestHeaders { get; set; } = {
			DefaultHeaderName,
			"X-Request-ID"
		};

		/// <summary>
		/// Gets or sets whether to include the correlation id in the response.
		/// </summary>
		public bool IncludeInResponse { get; set; } = true;
	}
}