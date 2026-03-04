using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class MLKEMTests
{
    [Fact]
    public void TestMlkem512()
    {
        var (privKey, pubKey) = MLKEMLevel.MLKEM512.Keypair();
        Assert.Equal(1632, privKey.AsBytes().Length);
        Assert.Equal(800, pubKey.AsBytes().Length);

        var (sharedSecret, ciphertext) = pubKey.EncapsulateNewSharedSecret();
        Assert.Equal(768, ciphertext.AsBytes().Length);
        Assert.Equal(32, sharedSecret.Data.Length);

        var recovered = privKey.DecapsulateSharedSecret(ciphertext);
        Assert.Equal(sharedSecret, recovered);
    }

    [Fact]
    public void TestMlkem768()
    {
        var (privKey, pubKey) = MLKEMLevel.MLKEM768.Keypair();
        Assert.Equal(2400, privKey.AsBytes().Length);
        Assert.Equal(1184, pubKey.AsBytes().Length);

        var (sharedSecret, ciphertext) = pubKey.EncapsulateNewSharedSecret();
        Assert.Equal(1088, ciphertext.AsBytes().Length);
        Assert.Equal(32, sharedSecret.Data.Length);

        var recovered = privKey.DecapsulateSharedSecret(ciphertext);
        Assert.Equal(sharedSecret, recovered);
    }

    [Fact]
    public void TestMlkem1024()
    {
        var (privKey, pubKey) = MLKEMLevel.MLKEM1024.Keypair();
        Assert.Equal(3168, privKey.AsBytes().Length);
        Assert.Equal(1568, pubKey.AsBytes().Length);

        var (sharedSecret, ciphertext) = pubKey.EncapsulateNewSharedSecret();
        Assert.Equal(1568, ciphertext.AsBytes().Length);
        Assert.Equal(32, sharedSecret.Data.Length);

        var recovered = privKey.DecapsulateSharedSecret(ciphertext);
        Assert.Equal(sharedSecret, recovered);
    }
}
