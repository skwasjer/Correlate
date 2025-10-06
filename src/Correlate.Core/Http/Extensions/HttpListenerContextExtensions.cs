using Correlate.Http.Server;

namespace Correlate.Http.Extensions;

internal static class HttpListenerContextExtensions
{
    internal static KeyValuePair<string, string?[]?> GetCorrelationIdHeader(this IHttpListenerContext context, IReadOnlyCollection<string> acceptedHeaders)
    {
        if (acceptedHeaders is null)
        {
            throw new ArgumentNullException(nameof(acceptedHeaders));
        }

        if (acceptedHeaders.Count == 0)
        {
            return new KeyValuePair<string, string?[]?>(CorrelationHttpHeaders.CorrelationId, null);
        }

        string?[]? correlationId = null;
        string? headerName = null;

        foreach (string requestHeaderName in acceptedHeaders)
        {
            if (!context.TryGetRequestHeader(requestHeaderName, out string?[]? value))
            {
                continue;
            }

            headerName = requestHeaderName;
            correlationId = value;
            if (correlationId?.Length > 0)
            {
                break;
            }
        }

        return new KeyValuePair<string, string?[]?>(
            headerName ?? acceptedHeaders.First(),
            correlationId
        );
    }
}
