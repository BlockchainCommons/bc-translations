using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.KnownValues.Tests;

public sealed class KnownValueTests
{
    [Fact]
    public void NewWithAndWithoutNameExposeExpectedProperties()
    {
        var unnamed = new KnownValue(42);
        Assert.Equal(42ul, unnamed.Value);
        Assert.Null(unnamed.AssignedName);
        Assert.Equal("42", unnamed.Name);

        var named = KnownValue.NewWithName(1u, "isA");
        Assert.Equal(1ul, named.Value);
        Assert.Equal("isA", named.AssignedName);
        Assert.Equal("isA", named.Name);
    }

    [Fact]
    public void EqualityAndHashingIgnoreAssignedName()
    {
        var first = KnownValue.NewWithName(1u, "isA");
        var second = KnownValue.NewWithName(1u, "different");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        var clone = first.Clone();
        Assert.Equal(first, clone);
        Assert.NotSame(first, clone);
    }

    [Fact]
    public void ToStringAndImplicitConversionUseDisplayNameFallback()
    {
        KnownValue converted = 99;
        Assert.Equal("99", converted.ToString());

        var named = KnownValue.NewWithName(100u, "customValue");
        Assert.Equal("customValue", named.ToString());
    }

    [Fact]
    public void TaggedCborRoundTripMatchesExpectedEncoding()
    {
        var cbor = KnownValuesRegistry.IsA.TaggedCbor();
        Assert.Equal("d99c4001", Convert.ToHexString(cbor.ToCborData()).ToLowerInvariant());

        var decoded = KnownValue.FromTaggedCbor(cbor);
        Assert.Equal(KnownValuesRegistry.IsA, decoded);

        var untaggedDecoded = KnownValue.FromUntaggedCbor(Cbor.FromUInt(1ul));
        Assert.Equal(KnownValuesRegistry.IsA, untaggedDecoded);
    }

    [Fact]
    public void DigestMatchesTaggedCborDigest()
    {
        var digest = KnownValuesRegistry.IsA.GetDigest();
        Assert.Equal(
            "2be2d79b306a21ff8e3e6bd3d1c2c6c74ff4a693b1e7ba3a0f40cdfb9ea493f8",
            digest.Hex);
    }
}
