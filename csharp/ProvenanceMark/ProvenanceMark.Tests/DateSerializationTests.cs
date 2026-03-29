using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class DateSerializationTests
{
    [Fact]
    public void Test2ByteDates()
    {
        var baseDate = CborDate.FromYmdHms(2023, 6, 20, 0, 0, 0);
        var serialized = DateSerialization.Serialize2Bytes(baseDate);
        Assert.Equal("00d4", Util.ToHex(serialized));
        Assert.Equal(baseDate, DateSerialization.Deserialize2Bytes(serialized));

        var minSerialized = new byte[] { 0x00, 0x21 };
        var minDate = CborDate.FromYmdHms(2023, 1, 1, 0, 0, 0);
        Assert.Equal(minDate, DateSerialization.Deserialize2Bytes(minSerialized));

        var maxSerialized = new byte[] { 0xff, 0x9f };
        var maxDate = CborDate.FromYmdHms(2150, 12, 31, 0, 0, 0);
        Assert.Equal(maxDate, DateSerialization.Deserialize2Bytes(maxSerialized));

        Assert.Throws<ProvenanceMarkException>(() => DateSerialization.Deserialize2Bytes(new byte[] { 0x00, 0x5e }));
    }

    [Fact]
    public void Test4ByteDates()
    {
        var baseDate = CborDate.FromDateTime(new DateTimeOffset(2023, 6, 20, 12, 34, 56, TimeSpan.Zero));
        var serialized = DateSerialization.Serialize4Bytes(baseDate);
        Assert.Equal(TestSupport.Hex("2a41d470"), serialized);
        Assert.Equal(baseDate, DateSerialization.Deserialize4Bytes(serialized));

        var minSerialized = TestSupport.Hex("00000000");
        var minDate = CborDate.FromYmdHms(2001, 1, 1, 0, 0, 0);
        Assert.Equal(minDate, DateSerialization.Deserialize4Bytes(minSerialized));

        var maxSerialized = TestSupport.Hex("ffffffff");
        var maxDate = CborDate.FromDateTime(new DateTimeOffset(2137, 2, 7, 6, 28, 15, TimeSpan.Zero));
        Assert.Equal(maxDate, DateSerialization.Deserialize4Bytes(maxSerialized));
    }

    [Fact]
    public void Test6ByteDates()
    {
        var baseDate = CborDate.FromDateTime(new DateTimeOffset(2023, 6, 20, 12, 34, 56, 789, TimeSpan.Zero));
        var serialized = DateSerialization.Serialize6Bytes(baseDate);
        Assert.Equal(TestSupport.Hex("00a51125d895"), serialized);
        Assert.Equal(baseDate, DateSerialization.Deserialize6Bytes(serialized));

        var minSerialized = TestSupport.Hex("000000000000");
        var minDate = CborDate.FromYmdHms(2001, 1, 1, 0, 0, 0);
        Assert.Equal(minDate, DateSerialization.Deserialize6Bytes(minSerialized));

        var maxSerialized = TestSupport.Hex("e5940a78a7ff");
        var maxDate = CborDate.FromDateTime(new DateTimeOffset(9999, 12, 31, 23, 59, 59, 999, TimeSpan.Zero));
        Assert.Equal(maxDate, DateSerialization.Deserialize6Bytes(maxSerialized));

        Assert.Throws<ProvenanceMarkException>(() => DateSerialization.Deserialize6Bytes(TestSupport.Hex("e5940a78a800")));
    }
}
