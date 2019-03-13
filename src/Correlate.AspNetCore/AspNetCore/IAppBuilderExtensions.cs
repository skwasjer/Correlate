using Correlate.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Correlate.AspNetCore
{
	// ReSharper disable once InconsistentNaming
	public static class IAppBuilderExtensions
	{
		public static IApplicationBuilder UseCorrelate(this IApplicationBuilder appBuilder)
		{
			appBuilder.UseMiddleware<CorrelateMiddleware>();
			return appBuilder;
		}
	}
}
