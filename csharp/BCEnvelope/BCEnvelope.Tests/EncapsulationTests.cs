using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class EncapsulationTests
{
    private static void TestScheme(EncapsulationScheme scheme)
    {
        var (privateKey, publicKey) = scheme.Keypair();
        var envelope = TestData.HelloEnvelope();
        var encryptedEnvelope = envelope
            .EncryptToRecipient(publicKey)
            .CheckEncoding();
        var decryptedEnvelope = encryptedEnvelope
            .DecryptToRecipient(privateKey);
        Assert.Equal(
            envelope.StructuralDigest(),
            decryptedEnvelope.StructuralDigest());
    }

    [Fact]
    public void TestEncapsulation()
    {
        TagsRegistry.RegisterTags();

        TestScheme(EncapsulationScheme.X25519);
        TestScheme(EncapsulationScheme.MLKEM512);
        TestScheme(EncapsulationScheme.MLKEM768);
        TestScheme(EncapsulationScheme.MLKEM1024);
    }
}
