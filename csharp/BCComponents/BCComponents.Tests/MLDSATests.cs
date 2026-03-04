using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class MLDSATests
{
    private readonly byte[] _message =
        Encoding.UTF8.GetBytes(
            "Ladies and Gentlemen of the class of '99: " +
            "If I could offer you only one tip for the future, sunscreen would be it.");

    [Fact]
    public void TestMldsa44Signing()
    {
        var (privKey, pubKey) = MLDSALevel.MLDSA44.Keypair();
        Assert.Equal(MLDSALevel.MLDSA44.PrivateKeySize(), privKey.AsBytes().Length);
        Assert.Equal(MLDSALevel.MLDSA44.PublicKeySize(), pubKey.AsBytes().Length);

        var sig = privKey.Sign(_message);
        Assert.Equal(MLDSALevel.MLDSA44.SignatureSize(), sig.AsBytes().Length);
        Assert.True(pubKey.Verify(sig, _message));

        // Modified message should fail verification
        var modified = (byte[])_message.Clone();
        modified[0] = (byte)(modified[0] + 1);
        Assert.False(pubKey.Verify(sig, modified));
    }

    [Fact]
    public void TestMldsa65Signing()
    {
        var (privKey, pubKey) = MLDSALevel.MLDSA65.Keypair();
        Assert.Equal(MLDSALevel.MLDSA65.PrivateKeySize(), privKey.AsBytes().Length);
        Assert.Equal(MLDSALevel.MLDSA65.PublicKeySize(), pubKey.AsBytes().Length);

        var sig = privKey.Sign(_message);
        Assert.Equal(MLDSALevel.MLDSA65.SignatureSize(), sig.AsBytes().Length);
        Assert.True(pubKey.Verify(sig, _message));

        var modified = (byte[])_message.Clone();
        modified[0] = (byte)(modified[0] + 1);
        Assert.False(pubKey.Verify(sig, modified));
    }

    [Fact]
    public void TestMldsa87Signing()
    {
        var (privKey, pubKey) = MLDSALevel.MLDSA87.Keypair();
        Assert.Equal(MLDSALevel.MLDSA87.PrivateKeySize(), privKey.AsBytes().Length);
        Assert.Equal(MLDSALevel.MLDSA87.PublicKeySize(), pubKey.AsBytes().Length);

        var sig = privKey.Sign(_message);
        Assert.Equal(MLDSALevel.MLDSA87.SignatureSize(), sig.AsBytes().Length);
        Assert.True(pubKey.Verify(sig, _message));

        var modified = (byte[])_message.Clone();
        modified[0] = (byte)(modified[0] + 1);
        Assert.False(pubKey.Verify(sig, modified));
    }

    [Fact]
    public void TestMldsaCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var (privKey, pubKey) = MLDSALevel.MLDSA65.Keypair();
        var sig = privKey.Sign(_message);

        // Private key roundtrip
        var privDecoded = MLDSAPrivateKey.FromTaggedCbor(privKey.TaggedCbor());
        Assert.Equal(privKey.Level, privDecoded.Level);
        Assert.Equal(privKey.AsBytes(), privDecoded.AsBytes());

        // Public key roundtrip
        var pubDecoded = MLDSAPublicKey.FromTaggedCbor(pubKey.TaggedCbor());
        Assert.Equal(pubKey.Level, pubDecoded.Level);
        Assert.Equal(pubKey.AsBytes(), pubDecoded.AsBytes());

        // Signature roundtrip
        var sigDecoded = MLDSASignature.FromTaggedCbor(sig.TaggedCbor());
        Assert.Equal(sig.Level, sigDecoded.Level);
        Assert.Equal(sig.AsBytes(), sigDecoded.AsBytes());
    }
}
