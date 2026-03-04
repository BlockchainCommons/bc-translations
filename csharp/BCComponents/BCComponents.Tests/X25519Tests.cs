using BlockchainCommons.BCComponents;
using BlockchainCommons.BCRand;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class X25519Tests
{
    [Fact]
    public void TestAgreement()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();

        var alicePrivateKey = X25519PrivateKey.NewUsing(rng);
        var alicePublicKey = alicePrivateKey.PublicKey();

        var bobPrivateKey = X25519PrivateKey.NewUsing(rng);
        var bobPublicKey = bobPrivateKey.PublicKey();

        var aliceSharedKey = alicePrivateKey.SharedKeyWith(bobPublicKey);
        var bobSharedKey = bobPrivateKey.SharedKeyWith(alicePublicKey);

        Assert.Equal(aliceSharedKey, bobSharedKey);
    }

    [Fact]
    public void TestPrivateKeyCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var rng = SeededRandomNumberGenerator.CreateFake();
        var privateKey = X25519PrivateKey.NewUsing(rng);
        var cbor = privateKey.TaggedCbor();
        var decoded = X25519PrivateKey.FromTaggedCbor(cbor);
        Assert.Equal(privateKey, decoded);
    }

    [Fact]
    public void TestPublicKeyCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var rng = SeededRandomNumberGenerator.CreateFake();
        var privateKey = X25519PrivateKey.NewUsing(rng);
        var publicKey = privateKey.PublicKey();
        var cbor = publicKey.TaggedCbor();
        var decoded = X25519PublicKey.FromTaggedCbor(cbor);
        Assert.Equal(publicKey, decoded);
    }

    [Fact]
    public void TestKeypairUsing()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var (privateKey, publicKey) = X25519PrivateKey.KeypairUsing(rng);
        Assert.Equal(publicKey, privateKey.PublicKey());
    }

    [Fact]
    public void TestDeriveFromKeyMaterial()
    {
        var keyMaterial = System.Text.Encoding.UTF8.GetBytes("password");
        var key1 = X25519PrivateKey.DeriveFromKeyMaterial(keyMaterial);
        var key2 = X25519PrivateKey.DeriveFromKeyMaterial(keyMaterial);
        Assert.Equal(key1, key2);
    }
}
