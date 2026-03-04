using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class ECKeyTests
{
    private const string PrivateKeyHex = "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36";

    [Fact]
    public void TestEcPrivateKeyFromHex()
    {
        var key = ECPrivateKey.FromHex(PrivateKeyHex);
        Assert.Equal(PrivateKeyHex, key.Hex);
        Assert.Equal(32, key.Data.Length);
    }

    [Fact]
    public void TestEcPublicKeyDerivation()
    {
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var pubKey = privKey.PublicKey();
        Assert.Equal(33, pubKey.Data.Length);
    }

    [Fact]
    public void TestSchnorrPublicKeyDerivation()
    {
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var schnorrPubKey = privKey.SchnorrPublicKey();
        Assert.Equal(32, schnorrPubKey.Data.Length);
    }

    [Fact]
    public void TestEcdsaSignAndVerify()
    {
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var pubKey = privKey.PublicKey();
        var message = Encoding.UTF8.GetBytes("Hello, World!");

        var signature = privKey.EcdsaSign(message);
        Assert.Equal(64, signature.Length);
        Assert.True(pubKey.Verify(signature, message));
    }

    [Fact]
    public void TestSchnorrSignAndVerify()
    {
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var schnorrPubKey = privKey.SchnorrPublicKey();
        var message = Encoding.UTF8.GetBytes("Hello, World!");

        var signature = privKey.SchnorrSign(message);
        Assert.Equal(64, signature.Length);
        Assert.True(schnorrPubKey.SchnorrVerify(signature, message));
    }

    [Fact]
    public void TestEcPrivateKeyCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var key = ECPrivateKey.FromHex(PrivateKeyHex);
        var cbor = key.TaggedCbor();
        var decoded = ECPrivateKey.FromTaggedCbor(cbor);
        Assert.Equal(key, decoded);
    }

    [Fact]
    public void TestEcPublicKeyCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var pubKey = privKey.PublicKey();
        var cbor = pubKey.TaggedCbor();
        var decoded = ECPublicKey.FromTaggedCbor(cbor);
        Assert.Equal(pubKey, decoded);
    }

    [Fact]
    public void TestUncompressedPublicKey()
    {
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var pubKey = privKey.PublicKey();
        var uncompressed = pubKey.UncompressedPublicKey();
        Assert.Equal(65, uncompressed.Data.Length);
    }

    [Fact]
    public void TestUncompressedToCompressedRoundtrip()
    {
        var privKey = ECPrivateKey.FromHex(PrivateKeyHex);
        var pubKey = privKey.PublicKey();
        var uncompressed = pubKey.UncompressedPublicKey();
        var recompressed = uncompressed.PublicKey();
        Assert.Equal(pubKey, recompressed);
    }

    [Fact]
    public void TestEd25519SignAndVerify()
    {
        var privKey = Ed25519PrivateKey.New();
        var pubKey = privKey.PublicKey();
        var message = Encoding.UTF8.GetBytes("Hello, World!");

        var signature = privKey.Sign(message);
        Assert.Equal(64, signature.Length);
        Assert.True(pubKey.Verify(signature, message));
    }

    [Fact]
    public void TestEd25519DeterministicDerivation()
    {
        var keyMaterial = Encoding.UTF8.GetBytes("test-key-material");
        var key1 = Ed25519PrivateKey.DeriveFromKeyMaterial(keyMaterial);
        var key2 = Ed25519PrivateKey.DeriveFromKeyMaterial(keyMaterial);
        Assert.Equal(key1, key2);
        Assert.Equal(key1.PublicKey(), key2.PublicKey());
    }

    [Fact]
    public void TestEcPrivateKeyDeterministicDerivation()
    {
        var keyMaterial = Encoding.UTF8.GetBytes("test-key-material");
        var key1 = ECPrivateKey.DeriveFromKeyMaterial(keyMaterial);
        var key2 = ECPrivateKey.DeriveFromKeyMaterial(keyMaterial);
        Assert.Equal(key1, key2);
        Assert.Equal(key1.PublicKey(), key2.PublicKey());
    }
}
