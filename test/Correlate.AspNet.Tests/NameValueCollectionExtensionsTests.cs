using System.Collections.Specialized;
using Correlate.AspNet.Extensions;
using Correlate.Http;

namespace Correlate.AspNet.Tests;

public class NameValueCollectionExtensionsTests
{
    [Fact]
    public void GetCorrelationIdHeader_NullAcceptedHeaders_ThrowsArgumentNullException()
    {
        // Arrange
        var headers = new NameValueCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            headers.GetCorrelationIdHeader(null!));
    }

    [Fact]
    public void GetCorrelationIdHeader_EmptyAcceptedHeaders_ReturnsDefaultHeader()
    {
        // Arrange
        var headers = new NameValueCollection();
        string[] acceptedHeaders = [];

        // Act
        KeyValuePair<string, string?> result = headers.GetCorrelationIdHeader(acceptedHeaders);

        // Assert
        Assert.Equal(CorrelationHttpHeaders.CorrelationId, result.Key);
        Assert.Null(result.Value);
    }

    [Fact]
    public void GetCorrelationIdHeader_HeaderExists_ReturnsCorrectValue()
    {
        // Arrange
        var headers = new NameValueCollection();
        string headerName = "X-Correlation-ID";
        string correlationId = "test-correlation-id";
        headers.Add(headerName, correlationId);
        string[] acceptedHeaders = [headerName];

        // Act
        KeyValuePair<string, string?> result = headers.GetCorrelationIdHeader(acceptedHeaders);

        // Assert
        Assert.Equal(headerName, result.Key);
        Assert.Equal(correlationId, result.Value);
    }

    [Fact]
    public void GetCorrelationIdHeader_MultipleHeaderValues_ReturnsLastValue()
    {
        // Arrange
        var headers = new NameValueCollection();
        string headerName = "X-Correlation-ID";
        headers.Add(headerName, "first-value");
        headers.Add(headerName, "last-value");
        string[] acceptedHeaders = [headerName];

        // Act
        KeyValuePair<string, string?> result = headers.GetCorrelationIdHeader(acceptedHeaders);

        // Assert
        Assert.Equal(headerName, result.Key);
        Assert.Equal("last-value", result.Value);
    }

    [Fact]
    public void GetCorrelationIdHeader_MultipleAcceptedHeaders_ReturnsFirstFoundValue()
    {
        // Arrange
        var headers = new NameValueCollection();
        string firstHeader = "X-Correlation-ID";
        string secondHeader = "Request-ID";
        headers.Add(secondHeader, "second-value");
        string[] acceptedHeaders = [firstHeader, secondHeader];

        // Act
        KeyValuePair<string, string?> result = headers.GetCorrelationIdHeader(acceptedHeaders);

        // Assert
        Assert.Equal(secondHeader, result.Key);
        Assert.Equal("second-value", result.Value);
    }

    [Fact]
    public void TryAdd_NullCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((NameValueCollection)null!).TryAdd("key", "value"));
    }

    [Fact]
    public void TryAdd_NullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var collection = new NameValueCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            collection.TryAdd(null!, "value"));
    }

    [Fact]
    public void TryAdd_NewKey_ReturnsTrue()
    {
        // Arrange
        var collection = new NameValueCollection();

        // Act
        bool result = collection.TryAdd("key", "value");

        // Assert
        Assert.True(result);
        Assert.Equal("value", collection["key"]);
    }

    [Fact]
    public void TryAdd_ExistingKey_ReturnsFalse()
    {
        // Arrange
        var collection = new NameValueCollection
        {
            { "key", "original-value" }
        };

        // Act
        bool result = collection.TryAdd("key", "new-value");

        // Assert
        Assert.False(result);
        Assert.Equal("original-value", collection["key"]);
    }

    [Fact]
    public void TryAdd_ExistingKeyDifferentCase_ReturnsFalse()
    {
        // Arrange
        var collection = new NameValueCollection
        {
            { "KEY", "original-value" }
        };

        // Act
        bool result = collection.TryAdd("key", "new-value");

        // Assert
        Assert.False(result);
        Assert.Equal("original-value", collection["key"]);
    }
}
