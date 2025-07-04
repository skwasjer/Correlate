using System.Collections;
using Correlate.AspNet.Extensions;

namespace Correlate.AspNet.Tests;

public class DictionaryExtensionsTests
{
    [Fact]
    public void TryGetValue_WhenDictionaryIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IDictionary? dictionary = null;
        string key = "test";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary!.TryGetValue(key, out string? _));
    }

    [Fact]
    public void TryGetValue_WhenKeyIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IDictionary dictionary = new Hashtable();
        string? key = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary.TryGetValue(key!, out string? _));
    }

    [Fact]
    public void TryGetValue_WhenKeyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        IDictionary dictionary = new Hashtable();
        string key = "nonexistent";

        // Act
        bool result = dictionary.TryGetValue(key, out string? value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_WhenKeyExistsButWrongType_ReturnsFalse()
    {
        // Arrange
        IDictionary dictionary = new Hashtable();
        string key = "test";
        dictionary[key] = 42; // Integer instead of string

        // Act
        bool result = dictionary.TryGetValue(key, out string? value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_WhenKeyExistsAndCorrectType_ReturnsTrue()
    {
        // Arrange
        IDictionary dictionary = new Hashtable();
        string key = "test";
        string expectedValue = "value";
        dictionary[key] = expectedValue;

        // Act
        bool result = dictionary.TryGetValue(key, out string? value);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void TryGetValue_WithNumericTypes_WorksCorrectly()
    {
        // Arrange
        IDictionary dictionary = new Hashtable();
        string key = "number";
        int expectedValue = 42;
        dictionary[key] = expectedValue;

        // Act
        bool result = dictionary.TryGetValue(key, out int value);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, value);
    }
}
