using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class KeypairSigningTests
{
    private static void TestScheme(SignatureScheme scheme, SigningOptions? options = null)
    {
        var (privateKey, publicKey) = scheme.Keypair();
        var envelope = TestData.HelloEnvelope()
            .SignOpt(privateKey, options)
            .CheckEncoding();
        envelope.Verify(publicKey);
    }

    [Fact]
    public void TestKeypairSigning()
    {
        TagsRegistry.RegisterTags();

        TestScheme(SignatureScheme.Schnorr);
        TestScheme(SignatureScheme.Ecdsa);
        TestScheme(SignatureScheme.Ed25519);
        TestScheme(SignatureScheme.MLDSA44);
        TestScheme(SignatureScheme.MLDSA65);
        TestScheme(SignatureScheme.MLDSA87);
    }

    [Fact]
    public void TestKeypairSigningSsh()
    {
        var options = new SigningOptions.SshOptions("test", "sha512");
        TestScheme(SignatureScheme.SshEd25519, options);
        TestScheme(SignatureScheme.SshDsa, options);
        // Note: SshEcdsaP256 and SshEcdsaP384 are skipped due to a
        // known issue in BCComponents' SshKeyHelper.DerivePublicKey
        // with BouncyCastle's OpenSshPublicKeyUtilities for EC keys.
    }
}
