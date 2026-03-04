using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class JsonTests
{
    [Fact]
    public void TestJsonCreation()
    {
        var json = Json.FromString("{\"key\": \"value\"}");
        Assert.Equal("{\"key\": \"value\"}", json.AsString());
        Assert.Equal(16, json.Length);
        Assert.False(json.IsEmpty);
    }

    [Fact]
    public void TestJsonFromBytes()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("[1, 2, 3]");
        var json = Json.FromData(data);
        Assert.Equal(data, json.AsBytes().ToArray());
        Assert.Equal("[1, 2, 3]", json.AsString());
    }

    [Fact]
    public void TestJsonEmpty()
    {
        var json = Json.FromString("");
        Assert.True(json.IsEmpty);
        Assert.Equal(0, json.Length);
    }

    [Fact]
    public void TestJsonCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var json = Json.FromString("{\"name\":\"Alice\",\"age\":30}");
        var cbor = json.TaggedCbor();
        var json2 = Json.FromTaggedCbor(cbor);
        Assert.Equal(json, json2);
    }

    [Fact]
    public void TestJsonHex()
    {
        var json = Json.FromString("test");
        var hex = json.Hex;
        var json2 = Json.FromHex(hex);
        Assert.Equal(json, json2);
    }

    [Fact]
    public void TestJsonToString()
    {
        var json = Json.FromString("{\"test\":true}");
        var str = json.ToString();
        Assert.Equal("JSON({\"test\":true})", str);
    }

    [Fact]
    public void TestJsonClone()
    {
        var json = Json.FromString("original");
        var json2 = Json.FromData(json.ToByteArray());
        Assert.Equal(json, json2);
    }

    [Fact]
    public void TestJsonToByteArray()
    {
        var json = Json.FromString("data");
        var bytes = json.ToByteArray();
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes("data"), bytes);
    }
}
