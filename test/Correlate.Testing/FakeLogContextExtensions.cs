using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Correlate.Testing;

public static class FakeLogContextExtensions
{
    public static FakeLogContext CreateLoggerContext(this IServiceProvider serviceProvider)
    {
        ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("FakeLogger");
        return logger.CreateLoggerContext();
    }

    public static FakeLogContext CreateLoggerContext(this ILogger logger)
    {
        string id = Guid.NewGuid().ToString("D");
        return new FakeLogContext(id, logger.BeginScope(id));
    }

    public static IReadOnlyList<FakeLogRecord> GetSnapshot(this FakeLogCollector collector, FakeLogContext context, bool clear = false)
    {
        return collector.GetSnapshot(context.Id, clear);
    }

    public static IReadOnlyList<FakeLogRecord> GetSnapshot(this FakeLogCollector collector, string id, bool clear = false)
    {
        return collector.GetSnapshot(clear)
            .Where(e => e.Scopes.Contains(id))
            .ToList();
    }
}
