using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR.Tests;

/// <summary>
/// A test type implementing URCodable, matching the Rust test in ur_codable.rs.
/// </summary>
internal sealed class TestLeaf : ICborTagged, IURCodable
{
    public string S { get; }

    public TestLeaf(string s) => S = s;

    public static IReadOnlyList<Tag> CborTags => [new Tag(24, "leaf")];

    public Cbor UntaggedCbor() => Cbor.FromString(S);

    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    public Cbor ToCbor() => TaggedCbor();

    public static TestLeaf FromUntaggedCbor(Cbor cbor)
    {
        return new TestLeaf(cbor.TryIntoText());
    }

    public static TestLeaf FromTaggedCbor(Cbor cbor)
    {
        var item = cbor.TryIntoExpectedTaggedValue(CborTags[0]);
        return FromUntaggedCbor(item);
    }
}

public class URCodableTests
{
    [Fact]
    public void UrCodableRoundTrip()
    {
        var test = new TestLeaf("test");
        var ur = test.ToUR();
        var urString = ur.ToUrString();
        Assert.Equal("ur:leaf/iejyihjkjygupyltla", urString);

        var ur2 = UR.FromUrString(urString);
        ur2.CheckType("leaf");
        var test2 = TestLeaf.FromUntaggedCbor(ur2.Cbor);
        Assert.Equal(test.S, test2.S);
    }
}
