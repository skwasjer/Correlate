using System.Collections.Specialized;
using System.Web;

namespace Correlate.AspNet;

public sealed class HttpListenerContextTests
{
    private readonly HttpContextBase _httpContext;
    private readonly HttpListenerContext _sut;
    private Func<Task> _fireOnSendingHeadersAsync = () => Task.CompletedTask;

    public HttpListenerContextTests()
    {
        _httpContext = Substitute.For<HttpContextBase>();
        _httpContext.Request.Headers.Returns(new NameValueCollection());
        _httpContext.Response.Headers.Returns(new NameValueCollection());
        _httpContext.Response
            .When(m => m.AddOnSendingHeaders(Arg.Any<Action<HttpContextBase>>()))
            .Do(ci => _fireOnSendingHeadersAsync = () =>
            {
                ci.Arg<Action<HttpContextBase>>()(_httpContext);
                return Task.CompletedTask;
            });

        _sut = new HttpListenerContext(_httpContext);
    }

    [Theory]
    [InlineData("X-Single", new[] { "Value1" })]
    [InlineData("X-Multiple", new[] { "Value1", "Value2" })]
    public void Given_that_request_header_exists_when_getting_it_should_return_true_and_expected_value(string key, string[] values)
    {
        foreach (string v in values)
        {
            _httpContext.Request.Headers.Add(key, v);
        }

        // Act
        bool result = _sut.TryGetRequestHeader(key, out string?[]? actualValue);

        // Assert
        result.Should().BeTrue();
        actualValue.Should().BeEquivalentTo(values, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void Given_that_request_header_does_not_exist_when_getting_it_should_return_false()
    {
        _httpContext.Request.Headers.Clear();

        // Act
        bool result = _sut.TryGetRequestHeader("non-existing", out string?[]? actualValue);

        // Assert
        result.Should().BeFalse();
        actualValue.Should().BeNull();
    }

    [Fact]
    public void Given_that_response_header_exists_when_adding_it_should_return_false_and_not_add()
    {
        const string key = "X-Single";
        const string value = "Value1";
        const string existingValue = "existing-value";
        _httpContext.Response.Headers[key] = existingValue;

        // Act
        bool result = _sut.TryAddResponseHeader(key, [value]);

        // Assert
        result.Should().BeFalse();
        _httpContext.Response.Headers[key].Should().BeEquivalentTo(existingValue);
    }

    [Theory]
    [InlineData("X-Single", new[] { "Value1" })]
    [InlineData("X-Multiple", new[] { "Value1", "Value2" })]
    public void Given_that_response_header_does_not_exist_when_adding_it_should_return_true_and_add(string key, string[] values)
    {
        _httpContext.Response.Headers.Clear();

        // Act
        bool result = _sut.TryAddResponseHeader(key, values);

        // Assert
        result.Should().BeTrue();
        string[]? currentValues = _httpContext.Response.Headers.GetValues(key);
        currentValues.Should().BeEquivalentTo(values);
    }

    [Fact]
    public async Task Given_that_starting_callback_is_registered_when_sending_headers_it_should_call_our_callback()
    {
        Action onStartedCallback = Substitute.For<Action>();
        _sut.OnStartingResponse(onStartedCallback);

        // Act
        await _fireOnSendingHeadersAsync();

        // Assert
        onStartedCallback.Received(1).Invoke();
    }

    [Fact]
    public void When_registering_null_starting_callback_it_should_not_throw()
    {
        // Act
        Action act = () => _sut.OnStartingResponse(null!);

        // Assert
        act.Should().NotThrow();
    }
}
