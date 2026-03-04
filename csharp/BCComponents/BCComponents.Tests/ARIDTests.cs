using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class ARIDTests
{
    [Fact]
    public void TestCreate()
    {
        var arid = ARID.New();
        Assert.Equal(ARID.Size, arid.Data.Length);
    }

    [Fact]
    public void TestUniqueness()
    {
        var arid1 = ARID.New();
        var arid2 = ARID.New();
        Assert.NotEqual(arid1, arid2);
    }

    [Fact]
    public void TestFromData()
    {
        var data = new byte[ARID.Size];
        for (var i = 0; i < data.Length; i++) data[i] = (byte)i;
        var arid = ARID.FromData(data);
        Assert.Equal(ARID.Size, arid.Data.Length);
    }

    [Fact]
    public void TestInvalidSize()
    {
        var data = new byte[16];
        Assert.Throws<BCComponentsException>(() => ARID.FromData(data));
    }

    [Fact]
    public void TestHexRoundtrip()
    {
        var arid = ARID.New();
        var hex = arid.Hex;
        Assert.Equal(64, hex.Length); // 32 bytes = 64 hex chars
        var arid2 = ARID.FromHex(hex);
        Assert.Equal(arid, arid2);
    }

    [Fact]
    public void TestShortDescription()
    {
        var data = new byte[ARID.Size];
        for (var i = 0; i < data.Length; i++) data[i] = (byte)i;
        var arid = ARID.FromData(data);
        Assert.Equal("00010203", arid.ShortDescription());
    }

    [Fact]
    public void TestCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var arid = ARID.New();
        var cbor = arid.TaggedCbor();
        var decoded = ARID.FromTaggedCbor(cbor);
        Assert.Equal(arid, decoded);
    }

    [Fact]
    public void TestEquality()
    {
        var hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
        var arid1 = ARID.FromHex(hex);
        var arid2 = ARID.FromHex(hex);
        Assert.Equal(arid1, arid2);
    }

    [Fact]
    public void TestComparable()
    {
        var data1 = new byte[ARID.Size];
        var data2 = new byte[ARID.Size];
        Array.Fill(data2, (byte)1);
        var arid1 = ARID.FromData(data1);
        var arid2 = ARID.FromData(data2);
        Assert.True(arid1.CompareTo(arid2) < 0);
    }

    [Fact]
    public void TestToString()
    {
        var hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
        var arid = ARID.FromHex(hex);
        Assert.Equal($"ARID({hex})", arid.ToString());
    }
}
