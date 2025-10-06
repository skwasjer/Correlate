using Correlate.Http.Server;

namespace Correlate.Http.Extensions;

public sealed class HttpListenerContextExtensionsTests
{
    private const string TestHeaderName = "TestHeaderName";
    private const string CorrelationId = "ed5f4a01-e5ad-4ffe-a697-6c81d1a198dd";

    private readonly HttpListenerContextStub _sut;

    public HttpListenerContextExtensionsTests()
    {
        _sut = new HttpListenerContextStub();
    }

    [Theory]
    [InlineData("first-header")]
    [InlineData("second-header")]
    [InlineData("third-header")]
    public void Given_list_of_accepted_headers_when_request_contains_one_of_the_headers_it_should_get_value(string requestHeaderKey)
    {
        _sut.RequestHeaders.Add(requestHeaderKey, CorrelationId);

        // Act
        KeyValuePair<string, string?[]?> header = _sut.GetCorrelationIdHeader(["first-header", "second-header", "third-header"]);

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string?[]?>(requestHeaderKey, [CorrelationId]));
    }

    [Fact]
    public void When_getting_by_custom_header_name_should_get_value()
    {
        _sut.RequestHeaders.Add(TestHeaderName, CorrelationId);

        // Act
        KeyValuePair<string, string?[]?> header = _sut.GetCorrelationIdHeader([TestHeaderName]);

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string?[]?>(TestHeaderName, [CorrelationId]));
    }

    [Fact]
    public void When_getting_by_name_it_should_get_value()
    {
        _sut.RequestHeaders.Add(CorrelationHttpHeaders.CorrelationId, CorrelationId);

        // Act
        KeyValuePair<string, string?[]?> header = _sut.GetCorrelationIdHeader([CorrelationHttpHeaders.CorrelationId]);

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string?[]?>(CorrelationHttpHeaders.CorrelationId, [CorrelationId]));
    }

    [Fact]
    public void When_header_is_not_found_should_return_preferred_header_without_value()
    {
        _sut.RequestHeaders.Should().BeEmpty();

        // Act
        KeyValuePair<string, string?[]?> header = _sut.GetCorrelationIdHeader([TestHeaderName]);

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string?[]?>(TestHeaderName, null));
    }

    [Fact]
    public void When_using_empty_accepted_headers_should_throw()
    {
        _sut.RequestHeaders.Add(CorrelationHttpHeaders.CorrelationId, CorrelationId);
        var expectedHeader = new KeyValuePair<string, string?[]?>(CorrelationHttpHeaders.CorrelationId, null);

        // Act
        KeyValuePair<string, string?[]?> header = _sut.GetCorrelationIdHeader([]);

        // Assert
        header.Should().BeEquivalentTo(expectedHeader, "it should not take correlation id from header dictionary but still return header key");
    }

    [Fact]
    public void When_using_null_for_accepted_headers_should_throw()
    {
        // Act
        Action act = () => _sut.GetCorrelationIdHeader(null!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .Which.ParamName.Should()
            .Be("acceptedHeaders");
    }
}
