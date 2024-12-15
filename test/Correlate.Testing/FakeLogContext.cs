namespace Correlate.Testing;

public sealed class FakeLogContext : IDisposable
{
    private readonly IDisposable? _logScope;

    internal FakeLogContext(string id, IDisposable? logScope)
    {
        Id = id;
        _logScope = logScope;
    }

    public string Id { get; }

    public void Dispose()
    {
        _logScope?.Dispose();
    }
}
