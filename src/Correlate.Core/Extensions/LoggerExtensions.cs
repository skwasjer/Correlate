﻿using System.Collections;
using Microsoft.Extensions.Logging;

namespace Correlate.Extensions;

internal static class LoggerExtensions
{
    public static IDisposable? BeginCorrelatedScope(this ILogger logger, string scopeKey, string correlationId)
    {
        return logger.BeginScope(new CorrelatedLogScope(scopeKey, correlationId));
    }

    private sealed class CorrelatedLogScope : IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly string _scopeKey;
        private readonly string _correlationId;

        public CorrelatedLogScope(string scopeKey, string correlationId)
        {
            _scopeKey = scopeKey;
            _correlationId = correlationId;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return this[0];
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return new KeyValuePair<string, object>(_scopeKey, _correlationId);
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
