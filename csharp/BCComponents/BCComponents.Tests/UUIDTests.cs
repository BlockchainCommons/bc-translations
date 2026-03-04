using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class UUIDTests
{
    [Fact]
    public void TestCreate()
    {
        var uuid = UUID.New();
        Assert.Equal(UUID.Size, uuid.Data.Length);
    }

    [Fact]
    public void TestUniqueness()
    {
        var uuid1 = UUID.New();
        var uuid2 = UUID.New();
        Assert.NotEqual(uuid1, uuid2);
    }

    [Fact]
    public void TestVersion4()
    {
        // Type 4 UUIDs have version nibble = 4 in byte 6
        var uuid = UUID.New();
        var data = uuid.Data;
        var versionNibble = (data[6] & 0xF0) >> 4;
        Assert.Equal(4, versionNibble);
    }

    [Fact]
    public void TestVariant2()
    {
        // RFC 4122 variant: top two bits of byte 8 = 10
        var uuid = UUID.New();
        var data = uuid.Data;
        var variantBits = (data[8] & 0xC0) >> 6;
        Assert.Equal(2, variantBits);
    }

    [Fact]
    public void TestStringFormat()
    {
        var data = Convert.FromHexString("0123456789abcdef0123456789abcdef");
        var uuid = UUID.FromData(data);
        var str = uuid.ToString();
        Assert.Matches(@"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", str);
        Assert.Equal("01234567-89ab-cdef-0123-456789abcdef", str);
    }

    [Fact]
    public void TestFromString()
    {
        var uuidString = "01234567-89ab-cdef-0123-456789abcdef";
        var uuid = UUID.FromString(uuidString);
        Assert.Equal(uuidString, uuid.ToString());
    }

    [Fact]
    public void TestFromData()
    {
        var data = new byte[UUID.Size];
        for (var i = 0; i < data.Length; i++) data[i] = (byte)i;
        var uuid = UUID.FromData(data);
        Assert.Equal(UUID.Size, uuid.Data.Length);
    }

    [Fact]
    public void TestCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var uuid = UUID.New();
        var cbor = uuid.TaggedCbor();
        var decoded = UUID.FromTaggedCbor(cbor);
        Assert.Equal(uuid, decoded);
    }

    [Fact]
    public void TestEquality()
    {
        var data = Convert.FromHexString("0123456789abcdef0123456789abcdef");
        var uuid1 = UUID.FromData(data);
        var uuid2 = UUID.FromData(data);
        Assert.Equal(uuid1, uuid2);
    }
}
