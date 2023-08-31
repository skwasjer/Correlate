﻿using Microsoft.AspNetCore.Http.Features;

namespace Correlate.AspNetCore;

/// <summary>
/// Copied from internal <see cref="Microsoft.AspNetCore.TestHost.ResponseFeature" /> implementation.
/// </summary>
internal class TestResponseFeature
    : IHttpResponseFeature,
      IDisposable,
      IAsyncDisposable
{
    private readonly HeaderDictionary _headers = new();
    private string? _reasonPhrase;
    private Func<Task> _responseCompletedAsync = () => Task.FromResult(true);
    private Func<Task> _responseStartingAsync = () => Task.FromResult(true);
    private int _statusCode;

    public TestResponseFeature()
    {
        Headers = _headers;
        Body = new MemoryStream();

        // 200 is the default status code all the way down to the host, so we set it
        // here to be consistent with the rest of the hosts when writing tests.
        StatusCode = 200;
    }

    public async ValueTask DisposeAsync()
    {
        await Body.DisposeAsync();
    }

    public void Dispose()
    {
        Body.Dispose();
    }

    public int StatusCode
    {
        get => _statusCode;
        set
        {
            if (HasStarted)
            {
                throw new InvalidOperationException("The status code cannot be set, the response has already started.");
            }

            if (value < 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The status code cannot be set to a value less than 100");
            }

            _statusCode = value;
        }
    }

    public string? ReasonPhrase
    {
        get => _reasonPhrase;
        set
        {
            if (HasStarted)
            {
                throw new InvalidOperationException("The reason phrase cannot be set, the response has already started.");
            }

            _reasonPhrase = value;
        }
    }

    public IHeaderDictionary Headers { get; set; }

    public Stream Body { get; set; }

    public bool HasStarted { get; set; }

    public void OnStarting(Func<object, Task> callback, object state)
    {
        if (HasStarted)
        {
            throw new InvalidOperationException();
        }

        Func<Task> prior = _responseStartingAsync;
        _responseStartingAsync = async () =>
        {
            await callback(state);
            await prior();
        };
    }

    public void OnCompleted(Func<object, Task> callback, object state)
    {
        Func<Task> prior = _responseCompletedAsync;
        _responseCompletedAsync = async () =>
        {
            try
            {
                await callback(state);
            }
            finally
            {
                await prior();
            }
        };
    }

    public async Task FireOnSendingHeadersAsync()
    {
        await _responseStartingAsync();
        HasStarted = true;
        _headers.IsReadOnly = true;
    }

    public Task FireOnResponseCompletedAsync()
    {
        return _responseCompletedAsync();
    }
}
