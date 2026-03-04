using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class EncapsulationTests
{
    [Fact]
    public void TestX25519Encapsulation()
    {
        var (privKey, pubKey) = EncapsulationScheme.X25519.Keypair();

        var (sharedKey, ct) = pubKey.EncapsulateNewSharedSecret();
        var recovered = privKey.DecapsulateSharedSecret(ct);
        Assert.Equal(sharedKey, recovered);
    }

    [Fact]
    public void TestMlkem512Encapsulation()
    {
        var (privKey, pubKey) = EncapsulationScheme.MLKEM512.Keypair();

        var (sharedKey, ct) = pubKey.EncapsulateNewSharedSecret();
        var recovered = privKey.DecapsulateSharedSecret(ct);
        Assert.Equal(sharedKey, recovered);
    }

    [Fact]
    public void TestMlkem768Encapsulation()
    {
        var (privKey, pubKey) = EncapsulationScheme.MLKEM768.Keypair();

        var (sharedKey, ct) = pubKey.EncapsulateNewSharedSecret();
        var recovered = privKey.DecapsulateSharedSecret(ct);
        Assert.Equal(sharedKey, recovered);
    }

    [Fact]
    public void TestMlkem1024Encapsulation()
    {
        var (privKey, pubKey) = EncapsulationScheme.MLKEM1024.Keypair();

        var (sharedKey, ct) = pubKey.EncapsulateNewSharedSecret();
        var recovered = privKey.DecapsulateSharedSecret(ct);
        Assert.Equal(sharedKey, recovered);
    }

    [Fact]
    public void TestSealedMessage()
    {
        TagsRegistry.RegisterTags();
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var (privKey, pubKey) = EncapsulationScheme.X25519.Keypair();

        var sealedMessage = SealedMessage.Create(plaintext, pubKey);
        var decrypted = sealedMessage.Decrypt(privKey);
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void TestSealedMessageMlkem()
    {
        TagsRegistry.RegisterTags();
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var (privKey, pubKey) = EncapsulationScheme.MLKEM768.Keypair();

        var sealedMessage = SealedMessage.Create(plaintext, pubKey);
        var decrypted = sealedMessage.Decrypt(privKey);
        Assert.Equal(plaintext, decrypted);
    }
}
