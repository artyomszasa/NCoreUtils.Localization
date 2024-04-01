using System.Text.Json;
using NCoreUtils.Localization.Serialization;
using Xunit;

namespace NCoreUtils.Localization.Unit;

public class SerializationTests
{
    protected const string JsonArray1 = @"[{ ""key"": ""key1"", ""value"": ""value1"" }, { ""key"": ""key2"", ""value"": ""value2"" }]";

    protected const string JsonArray2 = @"[{ ""value"": ""value1"", ""key"": ""key1"" }, { ""value"": ""value2"", ""key"": ""key2"" }]";

    protected const string JsonArray3 = @"[{ ""value"": ""value1"", ""key"": ""key1"", ""meta"": { ""verified"": true } }, { ""value"": ""value2"", ""key"": ""key2"" }]";

    protected const string JsonObject = @"{ ""key1"": ""value1"", ""key2"": ""value2"" }";

    protected const string JsonInvalid1 = "23";

    protected const string JsonNonStringKey = @"[{ ""key"": { ""xxx"": key1"" }, ""value"": ""value1"" }, { ""key"": ""key2"", ""value"": ""value2"" }]";

    protected const string JsonValueMissing = @"[{ ""key"": ""key1"" }, { ""key"": ""key2"", ""value"": ""value2"" }]";

    protected const string JsonKeyMissing = @"[{ ""value"": ""value1"" }, { ""key"": ""key2"", ""value"": ""value2"" }]";

    protected const string JsonMultipleKey = @"[{ ""key"": ""key1"", ""key"": ""key1"", ""value"": ""value1"" }, { ""key"": ""key2"", ""value"": ""value2"" }]";

    [Fact]
    public void Array1()
    {
        var map = JsonSerializer.Deserialize(JsonArray1, MapSerializerContext.Default.Map);
        Assert.NotNull(map);
        Assert.Equal(2, map!.Count);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key1"), out var value1));
        Assert.Equal("value1", value1);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key2"), out var value2));
        Assert.Equal("value2", value2);
    }

    [Fact]
    public void Array2()
    {
        var map = JsonSerializer.Deserialize(JsonArray2, MapSerializerContext.Default.Map);
        Assert.NotNull(map);
        Assert.Equal(2, map!.Count);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key1"), out var value1));
        Assert.Equal("value1", value1);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key2"), out var value2));
        Assert.Equal("value2", value2);
    }

    [Fact]
    public void Array3()
    {
        var map = JsonSerializer.Deserialize(JsonArray3, MapSerializerContext.Default.Map);
        Assert.NotNull(map);
        Assert.Equal(2, map!.Count);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key1"), out var value1));
        Assert.Equal("value1", value1);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key2"), out var value2));
        Assert.Equal("value2", value2);
    }

    [Fact]
    public void Object1()
    {
        var map = JsonSerializer.Deserialize(JsonObject, MapSerializerContext.Default.Map);
        Assert.NotNull(map);
        Assert.Equal(2, map!.Count);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key1"), out var value1));
        Assert.Equal("value1", value1);
        Assert.True(map!.TryGetValue(new CaseInsensitive("key2"), out var value2));
        Assert.Equal("value2", value2);
    }
}