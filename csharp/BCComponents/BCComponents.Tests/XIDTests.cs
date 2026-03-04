using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class XIDTests
{
    private const string SeedHex = "59f2293a5bce7d4de59e71b4207ac5d2";

    [Fact]
    public void TestXidFromKey()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);
        var pubKeys = ((IPublicKeysProvider)pkb).PublicKeys();
        var xid = XID.FromSigningPublicKey(pubKeys.SigningPublicKey);
        Assert.NotNull(xid);
        Assert.Equal(32, xid.Data.Length);
    }

    [Fact]
    public void TestXidValidate()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);
        var signingPubKey = pkb.DefaultSigningPublicKey();
        var xid = XID.FromSigningPublicKey(signingPubKey);

        Assert.True(xid.Validate(signingPubKey));
    }

    [Fact]
    public void TestXidCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);
        var xid = XID.FromSigningPublicKey(pkb.DefaultSigningPublicKey());

        var cbor = xid.TaggedCbor();
        var decoded = XID.FromTaggedCbor(cbor);
        Assert.Equal(xid, decoded);
    }

    [Fact]
    public void TestXidFromData()
    {
        var xidHex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037";
        var xid = XID.FromData(Convert.FromHexString(xidHex));
        Assert.Equal(32, xid.Data.Length);

        var cbor = xid.TaggedCbor();
        var decoded = XID.FromTaggedCbor(cbor);
        Assert.Equal(xid, decoded);
    }

    [Fact]
    public void TestXidFromHex()
    {
        TagsRegistry.RegisterTags();
        var xidHex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037";
        var xid = XID.FromHex(xidHex);
        Assert.Equal(xidHex, xid.Hex);
    }

    [Fact]
    public void TestXidEquality()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);
        var signingPubKey = pkb.DefaultSigningPublicKey();

        var xid1 = XID.FromSigningPublicKey(signingPubKey);
        var xid2 = XID.FromSigningPublicKey(signingPubKey);
        Assert.Equal(xid1, xid2);
    }

    [Fact]
    public void TestXidComparable()
    {
        var xid1 = XID.FromHex("0000000000000000000000000000000000000000000000000000000000000001");
        var xid2 = XID.FromHex("0000000000000000000000000000000000000000000000000000000000000002");
        Assert.True(xid1.CompareTo(xid2) < 0);
    }
}
