using Correlate.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Correlate.AspNetCore
{
	/// <summary>
	/// Registration extensions for <see cref="IApplicationBuilder"/>.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static class IAppBuilderExtensions
	{
		/// <summary>
		/// Adds Correlate middleware to the request execution pipeline.
		/// </summary>
		/// <param name="appBuilder">The <see cref="IApplicationBuilder"/>.</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
		public static IApplicationBuilder UseCorrelate(this IApplicationBuilder appBuilder)
		{
			appBuilder.UseMiddleware<CorrelateMiddleware>();
			return appBuilder;
		}
	}
}
