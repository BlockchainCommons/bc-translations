using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class PrivateKeyBaseTests
{
    private const string SeedHex = "59f2293a5bce7d4de59e71b4207ac5d2";

    [Fact]
    public void TestPrivateKeyBase()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);

        // Test signing key derivation
        var signingPrivKey = pkb.SigningPrivateKey();
        Assert.NotNull(signingPrivKey);
        Assert.Equal(32, signingPrivKey.Data.Length);

        // Test agreement key derivation
        var x25519PrivKey = pkb.X25519AgreementPrivateKey();
        Assert.NotNull(x25519PrivKey);

        // Test Ed25519 key derivation
        var ed25519PrivKey = pkb.Ed25519SigningPrivateKey();
        Assert.NotNull(ed25519PrivKey);
        Assert.Equal(32, ed25519PrivKey.Data.Length);
    }

    [Fact]
    public void TestPublicKeys()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);

        var publicKeys = ((IPublicKeysProvider)pkb).PublicKeys();
        Assert.NotNull(publicKeys.SigningPublicKey);
        Assert.NotNull(publicKeys.EncapsulationPublicKey);
    }

    [Fact]
    public void TestPrivateKeys()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);

        var privateKeys = ((IPrivateKeysProvider)pkb).PrivateKeys();
        Assert.NotNull(privateKeys);
    }

    [Fact]
    public void TestCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);

        var cbor = pkb.TaggedCbor();
        var decoded = PrivateKeyBase.FromTaggedCbor(cbor);
        Assert.Equal(pkb, decoded);
    }

    [Fact]
    public void TestDataRoundtrip()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb = PrivateKeyBase.FromData(seedData);

        var data = pkb.Data;
        var restored = PrivateKeyBase.FromData(data);
        Assert.Equal(pkb, restored);
    }

    [Fact]
    public void TestDeterministicDerivation()
    {
        var seedData = Convert.FromHexString(SeedHex);
        var pkb1 = PrivateKeyBase.FromData(seedData);
        var pkb2 = PrivateKeyBase.FromData(seedData);

        // Same seed produces same signing key
        Assert.Equal(pkb1.SigningPrivateKey(), pkb2.SigningPrivateKey());

        // Same seed produces same agreement key
        Assert.Equal(pkb1.X25519AgreementPrivateKey(), pkb2.X25519AgreementPrivateKey());

        // Same seed produces same Ed25519 key
        Assert.Equal(pkb1.Ed25519SigningPrivateKey(), pkb2.Ed25519SigningPrivateKey());
    }
}
