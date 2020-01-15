using Correlate.Http;

namespace Correlate.AspNetCore.Middleware
{
	/// <summary>
	/// Options for handling correlation id on incoming requests.
	/// </summary>
	public class CorrelateOptions
	{
		/// <summary>
		/// Gets or sets the request headers to retrieve the correlation id from.
		/// </summary>
		/// <remarks>
		/// The first matching header will be used.
		/// </remarks>
#pragma warning disable CA1819 // TODO: in next major release change to IEnumerable<>/ICollection<>.
		public string[] RequestHeaders { get; set; } = {
			CorrelationHttpHeaders.CorrelationId
		};
#pragma warning restore CA1819

		/// <summary>
		/// Gets or sets whether to include the correlation id in the response.
		/// </summary>
		/// <remarks>
		/// A common use case is to disable tracing info in edge services, so that such details are not exposed to the outside world.
		/// </remarks>
		public bool IncludeInResponse { get; set; } = true;
	}
}
