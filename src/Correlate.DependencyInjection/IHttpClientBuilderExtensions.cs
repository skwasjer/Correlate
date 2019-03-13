using System.Net.Http;
using Correlate.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Correlate.DependencyInjection
{
	// ReSharper disable once InconsistentNaming
	public static class IHttpClientBuilderExtensions
	{
		/// <summary>
		/// Adds services required for adding correlation id to each outgoing <see cref="HttpClient"/> request.
		/// </summary>
		/// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the services to.</param>
		/// <returns>The <see cref="IHttpClientBuilder"/> so that additional calls can be chained.</returns>
		public static IHttpClientBuilder CorrelateRequests(this IHttpClientBuilder builder)
		{
			builder.Services.AddCorrelate();

			builder.Services.TryAddTransient<CorrelatingHttpMessageHandler>();
			builder.AddHttpMessageHandler<CorrelatingHttpMessageHandler>();

			return builder;
		}
	}
}
