using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Correlate.Http.Extensions;

public class HeaderDictionaryExtensionsTests
{
    private const string TestHeaderName = "TestHeaderName";
    private static readonly string CorrelationId = Guid.NewGuid().ToString();

    private readonly Dictionary<string, StringValues> _sut;

    [Theory]
    [InlineData("first-header")]
    [InlineData("second-header")]
    [InlineData("third-header")]
    public void Given_list_of_accepted_headers_when_request_contains_one_of_the_headers_it_should_get_value(string requestHeaderKey)
    {
        _sut.Add(requestHeaderKey, new StringValues(CorrelationId));

        // Act
        KeyValuePair<string, string?> header = _sut.GetCorrelationIdHeader(new[] { "first-header", "second-header", "third-header" });

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string>(requestHeaderKey, CorrelationId));
    }

    public HeaderDictionaryExtensionsTests()
    {
        _sut = new Dictionary<string, StringValues>();
    }

    [Fact]
    public void When_getting_by_custom_header_name_should_get_value()
    {
        _sut.Add(TestHeaderName, new StringValues(CorrelationId));

        // Act
        KeyValuePair<string, string?> header = _sut.GetCorrelationIdHeader(new[] { TestHeaderName });

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string>(TestHeaderName, CorrelationId));
    }

    [Fact]
    public void When_getting_by_name_it_should_get_value()
    {
        _sut.Add(CorrelationHttpHeaders.CorrelationId, new StringValues(CorrelationId));

        // Act
        KeyValuePair<string, string?> header = _sut.GetCorrelationIdHeader(new[] { CorrelationHttpHeaders.CorrelationId });

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string>(CorrelationHttpHeaders.CorrelationId, CorrelationId));
    }

    [Fact]
    public void When_header_is_not_found_should_return_preferred_header_without_value()
    {
        // Act
        KeyValuePair<string, string?> header = _sut.GetCorrelationIdHeader(new[] { TestHeaderName });

        // Assert
        header.Should().BeEquivalentTo(new KeyValuePair<string, string?>(TestHeaderName, null));
    }

    [Fact]
    public void When_using_empty_accepted_headers_should_throw()
    {
        _sut.Add(CorrelationHttpHeaders.CorrelationId, new StringValues(CorrelationId));
        var expectedHeader = new KeyValuePair<string, string?>(CorrelationHttpHeaders.CorrelationId, null);

        // Act
        KeyValuePair<string, string?> header = _sut.GetCorrelationIdHeader(Array.Empty<string>());

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
