using Microsoft.Extensions.Logging;

namespace Correlate.Extensions;

public sealed class LoggerExtensionsTests
{
    private readonly ILogger _logger;

    public LoggerExtensionsTests()
    {
        _logger = Substitute.For<ILogger>();
    }

    [Theory]
    [InlineData(CorrelateConstants.CorrelationIdKey, "12345")]
    [InlineData("CustomKey", "abcdef")]
    public void When_beginning_scope_it_should_return_disposable_scope_implementing_kvp_list_containing_the_correlation_id
    (
        string scopeKey,
        string correlationId
    )
    {
        var expectedKvp = new KeyValuePair<string, object>(scopeKey, correlationId);

        Func<IReadOnlyList<KeyValuePair<string, object>>, bool> assertScope = kvps =>
        {
            kvps.Should()
                .ContainSingle()
                .Which.Should()
                .Be(expectedKvp);
            return true;
        };

        // Act
        _logger.BeginCorrelatedScope(scopeKey, correlationId);

        // Assert
        _logger.Received(1).BeginScope(Arg.Is<IReadOnlyList<KeyValuePair<string, object>>>(e => assertScope(e)));
    }

    [Theory]
    [InlineData(CorrelateConstants.CorrelationIdKey, "12345")]
    [InlineData("CustomKey", "abcdef")]
    public void When_formatting_scope_it_should_return_expected
    (
        string scopeKey,
        string correlationId
    )
    {
        string expectedStr = $"{scopeKey}:{correlationId}";

        Func<object, bool> assertScope = formattable =>
        {
            formattable.ToString().Should().Be(expectedStr);
            return true;
        };

        // Act
        _logger.BeginCorrelatedScope(scopeKey, correlationId);

        // Assert
        _logger.Received(1).BeginScope(Arg.Is<object>(e => assertScope(e)));
    }
}
