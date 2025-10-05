using Microsoft.AspNetCore.Http.Features;

namespace Correlate.AspNetCore;

public sealed class HttpListenerContextTests
{
    private readonly HttpRequestFeature _requestFeature;
    private readonly TestResponseFeature _responseFeature;
    private readonly HttpListenerContext _sut;

    public HttpListenerContextTests()
    {
        var httpContext = new DefaultHttpContext();
        _requestFeature = new HttpRequestFeature();
        _responseFeature = new TestResponseFeature();
        httpContext.Features.Set<IHttpRequestFeature>(_requestFeature);
        httpContext.Features.Set<IHttpResponseFeature>(_responseFeature);

        _sut = new HttpListenerContext(httpContext);
    }

    [Theory]
    [InlineData("X-Single", new[] { "Value1" }, "Value1")]
    [InlineData("X-Multiple", new[] { "Value1", "Value2" }, "Value1,Value2")]
    public void Given_that_request_header_exists_when_getting_it_should_return_true_and_expected_value(string key, string[] values, string? expectedValue)
    {
        _requestFeature.Headers[key] = values;

        // Act
        bool result = _sut.TryGetRequestHeader(key, out string? actualValue);

        // Assert
        result.Should().BeTrue();
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void Given_that_request_header_does_not_exist_when_getting_it_should_return_false()
    {
        _requestFeature.Headers.Clear();

        // Act
        bool result = _sut.TryGetRequestHeader("non-existing", out string? actualValue);

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
        _responseFeature.Headers[key] = existingValue;

        // Act
        bool result = _sut.TryAddResponseHeader(key, value);

        // Assert
        result.Should().BeFalse();
        _responseFeature.Headers[key].Should().BeEquivalentTo(existingValue);
    }

    [Fact]
    public void Given_that_response_header_does_not_exist_when_adding_it_should_return_true_and_add()
    {
        const string key = "X-Single";
        const string value = "Value1";
        _responseFeature.Headers.Clear();

        // Act
        bool result = _sut.TryAddResponseHeader(key, value);

        // Assert
        result.Should().BeTrue();
        _responseFeature.Headers[key].Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task Given_that_starting_callback_is_registered_when_sending_headers_it_should_call_our_callback()
    {
        Action onStartedCallback = Substitute.For<Action>();
        _sut.OnStartingResponse(onStartedCallback);

        // Act
        await _responseFeature.FireOnSendingHeadersAsync();

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
