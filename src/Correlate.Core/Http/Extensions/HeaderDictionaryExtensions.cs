namespace Correlate.Http.Extensions;

internal static class HeaderDictionaryExtensions
{
    internal static KeyValuePair<string, string?> GetCorrelationIdHeader<THeaderValue>(this IDictionary<string, THeaderValue> httpHeaders, IReadOnlyCollection<string> acceptedHeaders)
    {
        if (acceptedHeaders is null)
        {
            throw new ArgumentNullException(nameof(acceptedHeaders));
        }

        if (acceptedHeaders.Count == 0)
        {
            return new KeyValuePair<string, string?>(CorrelationHttpHeaders.CorrelationId, null);
        }

        string? correlationId = null;
        string? headerName = null;

        foreach (string requestHeaderName in acceptedHeaders)
        {
            if (!httpHeaders.TryGetValue(requestHeaderName, out THeaderValue? value))
            {
                continue;
            }

            headerName = requestHeaderName;
            correlationId = value is IEnumerable<string> multiValue
                ? multiValue.LastOrDefault()
                : value?.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                break;
            }
        }

        return new KeyValuePair<string, string?>(
            headerName ?? acceptedHeaders.First(),
            correlationId
        );
    }
}
