using System.Collections;

namespace Correlate.Internal;

public sealed class GenericDictionaryAdapterTests
{
    private readonly Hashtable _items;
    private readonly GenericDictionaryAdapter _sut;

    public GenericDictionaryAdapterTests()
    {
        _items = new Hashtable();
        _sut = new GenericDictionaryAdapter(_items);
    }

    [Fact]
    public void When_adding_it_should_add_kvp_to_items()
    {
        const string key = "key1";
        const string value = "value1";

        // Act
        _sut.Add(key, value);

        // Assert
        _items.ContainsKey(key).Should().BeTrue();
        _items[key].Should().Be(value);
    }

    [Fact]
    public void When_adding_kvp_it_should_add_kvp_to_items()
    {
        const string key = "key1";
        const string value = "value1";

        // Act
        _sut.Add(new KeyValuePair<object, object?>(key, value));

        // Assert
        _items.ContainsKey(key).Should().BeTrue();
        _items[key].Should().Be(value);
    }

    [Fact]
    public void Given_that_key_exists_when_removing_by_key_it_should_remove_kvp_from_items()
    {
        const string key = "key1";
        _items.Add(key, "value1");

        // Act
        bool result = _sut.Remove(key);

        // Assert
        result.Should().BeTrue();
        _items.ContainsKey(key).Should().BeFalse();
    }

    [Fact]
    public void Given_that_key_exists_when_removing_by_kvp_it_should_remove_kvp_from_items()
    {
        const string key = "key1";
        const string value = "value1";
        _items.Add(key, value);

        // Act
        var kvp = new KeyValuePair<object, object?>(key, value);
        bool result = _sut.Remove(kvp);

        // Assert
        result.Should().BeTrue();
        _items.ContainsKey(key).Should().BeFalse();
    }

    [Fact]
    public void Given_that_key_exists_but_value_is_different_when_removing_by_kvp_it_should_remove_kvp_from_items()
    {
        const string key = "key1";
        const string value = "value1";
        _items.Add(key, value);

        // Act
        var kvp = new KeyValuePair<object, object?>(key, "other-value");
        bool result = _sut.Remove(kvp);

        // Assert
        result.Should().BeFalse();
        _items.ContainsKey(key).Should().BeTrue();
    }

    [Fact]
    public void Given_that_key_does_not_exist_when_removing_it_should_return_false()
    {
        // Act
        bool result = _sut.Remove("non-existing");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_that_key_exists_when_checking_if_key_exists_it_should_return_true()
    {
        const string key = "key1";
        _items.Add(key, "value1");

        // Act
        bool result = _sut.ContainsKey(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_that_key_does_not_exist_when_checking_if_key_exists_it_should_return_false()
    {
        // Act
        bool result = _sut.ContainsKey("non-existing");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_that_key_exists_when_trying_to_get_it_should_return_true_with_value()
    {
        const string key = "key1";
        const string value = "value1";
        _items.Add(key, value);

        // Act
        bool result = _sut.TryGetValue(key, out object? actualValue);

        // Assert
        result.Should().BeTrue();
        actualValue.Should().Be(value);
    }

    [Fact]
    public void Given_that_key_does_not_exist_when_trying_to_get_it_should_return_false_with_default_value()
    {
        // Act
        bool result = _sut.TryGetValue("non-existing", out object? actualValue);

        // Assert
        result.Should().BeFalse();
        actualValue.Should().BeNull();
    }

    [Fact]
    public void Given_that_items_exist_when_clearing_it_should_remove_all_items()
    {
        _items.Add("key1", "value1");
        _items.Add("key2", "value2");

        // Act
        _sut.Clear();

        // Assert
        _items.Count.Should().Be(0);
    }

    [Fact]
    public void Given_that_items_exist_when_getting_count_it_should_return_expected()
    {
        _items.Add("key1", "value1");
        _items.Add("key2", "value2");

        // Act
        int count = _sut.Count;

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void When_setting_via_key_indexer_it_should_set_value()
    {
        const string key = "key1";
        const string value = "value1";

        // Act
        _sut[key] = value;

        // Assert
        _items[key].Should().Be(value);
    }

    [Fact]
    public void Given_that_key_exists_when_getting_via_key_indexer_it_should_return_value()
    {
        const string key = "key1";
        const string value = "value1";
        _items[key] = value;

        // Act
        object? actual = _sut[key];

        // Assert
        actual.Should().Be(value);
    }

    [Fact]
    public void Given_that_key_does_not_exist_when_getting_via_key_indexer_it_should_throw()
    {
        const string key = "key1";

        // Act
        Action act = () => _ = _sut[key];

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void When_iterating_over_all_items_it_should_return_expected()
    {
        _items.Add("key1", "value1");
        _items.Add("key2", "value2");
        KeyValuePair<object, object?>[] expected =
        [
            new("key1", "value1"),
            new("key2", "value2")
        ];

        // Act
        var result = _sut.ToList();
        IEnumerable<KeyValuePair<object, object?>>? resultEnum = _sut.AsEnumerable();

        // Assert
        result.Should().Equal(expected);
        resultEnum.Should().Equal(expected);
        _sut.Count.Should().Be(2);
    }

    [Fact]
    public void When_iterating_over_all_keys_it_should_return_expected()
    {
        _items.Add("key1", "value1");
        _items.Add("key2", "value2");
        object[] expected = ["key1", "key2"];

        // Act
        var result = _sut.Keys.ToList();
        IEnumerable<object>? resultEnum = _sut.Keys.AsEnumerable();

        // Assert
        result.Should().BeEquivalentTo(expected);
        resultEnum.Should().BeEquivalentTo(expected);
        _sut.Values.Count.Should().Be(2);
    }

    [Fact]
    public void When_iterating_over_all_values_it_should_return_expected()
    {
        _items.Add("key1", "value1");
        _items.Add("key2", "value2");
        object?[] expected = ["value1", "value2"];

        // Act
        var result = _sut.Values.ToList();
        IEnumerable<object?>? resultEnum = _sut.Values.AsEnumerable();

        // Assert
        result.Should().BeEquivalentTo(expected);
        resultEnum.Should().BeEquivalentTo(expected);
        _sut.Values.Count.Should().Be(2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void It_should_inherit_readonly_from_items(bool isReadOnly)
    {
        IDictionary? items = Substitute.For<IDictionary>();
        items.IsReadOnly.Returns(isReadOnly);
        var sut = new GenericDictionaryAdapter(items);

        // Act
        bool result = ((ICollection<KeyValuePair<object, object?>>)sut).IsReadOnly;

        // Assert
        result.Should().Be(isReadOnly);
    }

    [Fact]
    public void Keys_should_be_readonly()
    {
        _sut.Keys.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void Values_should_be_readonly()
    {
        _sut.Values.IsReadOnly.Should().BeTrue();
    }
}
