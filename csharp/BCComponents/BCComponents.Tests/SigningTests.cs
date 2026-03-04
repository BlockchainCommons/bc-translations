using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class SigningTests
{
    private const string EcdsaPrivateKeyHex = "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36";
    private readonly byte[] _message = Encoding.UTF8.GetBytes("Wolf McNally");

    [Fact]
    public void TestSchnorrSigning()
    {
        var ecPrivKey = ECPrivateKey.FromHex(EcdsaPrivateKeyHex);
        var signingPrivKey = SigningPrivateKey.NewSchnorr(ecPrivKey);
        var sig = signingPrivKey.SignWithOptions(_message);
        var signingPubKey = signingPrivKey.PublicKey();
        Assert.True(signingPubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestEcdsaSigning()
    {
        var ecPrivKey = ECPrivateKey.FromHex(EcdsaPrivateKeyHex);
        var signingPrivKey = SigningPrivateKey.NewEcdsa(ecPrivKey);
        var sig = signingPrivKey.SignWithOptions(_message);
        var signingPubKey = signingPrivKey.PublicKey();
        Assert.True(signingPubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestEd25519Signing()
    {
        var (privKey, pubKey) = SignatureScheme.Ed25519.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestSchnorrKeypair()
    {
        var (privKey, pubKey) = SignatureScheme.Schnorr.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestEcdsaKeypair()
    {
        var (privKey, pubKey) = SignatureScheme.Ecdsa.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestEd25519Keypair()
    {
        var (privKey, pubKey) = SignatureScheme.Ed25519.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestMldsa44Keypair()
    {
        var (privKey, pubKey) = SignatureScheme.MLDSA44.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestMldsa65Keypair()
    {
        var (privKey, pubKey) = SignatureScheme.MLDSA65.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestMldsa87Keypair()
    {
        var (privKey, pubKey) = SignatureScheme.MLDSA87.Keypair();
        var sig = privKey.SignWithOptions(_message);
        Assert.True(pubKey.Verify(sig, _message));
    }

    [Fact]
    public void TestSignatureCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var ecPrivKey = ECPrivateKey.FromHex(EcdsaPrivateKeyHex);

        // Schnorr
        var schnorrPriv = SigningPrivateKey.NewSchnorr(ecPrivKey);
        var schnorrSig = schnorrPriv.SignWithOptions(_message);
        var schnorrCbor = schnorrSig.TaggedCbor();
        var schnorrDecoded = Signature.FromTaggedCbor(schnorrCbor);
        Assert.Equal(schnorrSig, schnorrDecoded);

        // ECDSA
        var ecdsaPriv = SigningPrivateKey.NewEcdsa(ecPrivKey);
        var ecdsaSig = ecdsaPriv.SignWithOptions(_message);
        var ecdsaCbor = ecdsaSig.TaggedCbor();
        var ecdsaDecoded = Signature.FromTaggedCbor(ecdsaCbor);
        Assert.Equal(ecdsaSig, ecdsaDecoded);
    }

    [Fact]
    public void TestSigningPrivateKeyCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var ecPrivKey = ECPrivateKey.FromHex(EcdsaPrivateKeyHex);

        // Schnorr
        var schnorrKey = SigningPrivateKey.NewSchnorr(ecPrivKey);
        var schnorrDecoded = SigningPrivateKey.FromTaggedCbor(schnorrKey.TaggedCbor());
        Assert.Equal(schnorrKey, schnorrDecoded);

        // ECDSA
        var ecdsaKey = SigningPrivateKey.NewEcdsa(ecPrivKey);
        var ecdsaDecoded = SigningPrivateKey.FromTaggedCbor(ecdsaKey.TaggedCbor());
        Assert.Equal(ecdsaKey, ecdsaDecoded);
    }

    [Fact]
    public void TestSigningPublicKeyCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var ecPrivKey = ECPrivateKey.FromHex(EcdsaPrivateKeyHex);

        var schnorrPub = SigningPublicKey.FromSchnorr(ecPrivKey.SchnorrPublicKey());
        var schnorrDecoded = SigningPublicKey.FromTaggedCbor(schnorrPub.TaggedCbor());
        Assert.Equal(schnorrPub, schnorrDecoded);

        var ecdsaPub = SigningPublicKey.FromEcdsa(ecPrivKey.PublicKey());
        var ecdsaDecoded = SigningPublicKey.FromTaggedCbor(ecdsaPub.TaggedCbor());
        Assert.Equal(ecdsaPub, ecdsaDecoded);
    }
}
