using Correlate.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Correlate.DependencyInjection;

/// <summary>
/// Registration extensions for <see cref="IHttpClientBuilder" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds services required for adding correlation id to each outgoing <see cref="HttpClient" /> request.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder" /> to add the services to.</param>
    /// <param name="requestHeader">The request header name to set the correlation id in.</param>
    /// <returns>The <see cref="IHttpClientBuilder" /> so that additional calls can be chained.</returns>
    public static IHttpClientBuilder CorrelateRequests(this IHttpClientBuilder builder, string requestHeader = CorrelationHttpHeaders.CorrelationId)
    {
        return builder.CorrelateRequests(options => options.RequestHeader = requestHeader);
    }

    /// <summary>
    /// Adds services required for adding correlation id to each outgoing <see cref="HttpClient" /> request.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder" /> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure <see cref="CorrelateClientOptions" />.</param>
    /// <returns>The <see cref="IHttpClientBuilder" /> so that additional calls can be chained.</returns>
    public static IHttpClientBuilder CorrelateRequests(this IHttpClientBuilder builder, Action<CorrelateClientOptions> configureOptions)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddCorrelate();

        builder.Services.TryAddTransient<CorrelatingHttpMessageHandler>();
        builder.Services.Configure(builder.Name, configureOptions);
        builder.AddHttpMessageHandler(s =>
        {
            IOptionsSnapshot<CorrelateClientOptions> allClientOptions = s.GetRequiredService<IOptionsSnapshot<CorrelateClientOptions>>();
            IOptions<CorrelateClientOptions> thisClientOptions = Options.Create(allClientOptions.Get(builder.Name));

            return ActivatorUtilities.CreateInstance<CorrelatingHttpMessageHandler>(
                s,
                thisClientOptions
            );
        });

        return builder;
    }
}
