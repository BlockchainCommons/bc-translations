using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class SshTests
{
    [Fact]
    public void TestSshSignedPlaintext()
    {
        TagsRegistry.RegisterTags();

        // Generate SSH Ed25519 keypair
        var (aliceSshPrivateKey, aliceSshPublicKey) =
            SignatureScheme.SshEd25519.KeypairOpt("alice@example.com");
        var (carolSshPrivateKey, carolSshPublicKey) =
            SignatureScheme.SshEd25519.KeypairOpt("carol@example.com");

        // Alice sends a signed plaintext message to Bob.
        var options = new SigningOptions.SshOptions("test", "sha256");
        var envelope = TestData.HelloEnvelope()
            .AddSignatureOpt(aliceSshPrivateKey, options, null)
            .CheckEncoding();
        var ur = BCUR.UR.Create("envelope", envelope.TaggedCbor());

        var expectedFormat =
            "\"Hello.\" [\n" +
            "    'signed': Signature(SshEd25519)\n" +
            "]";
        Assert.Equal(expectedFormat, envelope.Format());

        // Bob receives the envelope.
        var receivedUr = BCUR.UR.FromUrString(ur.ToUrString());
        var receivedEnvelope = Envelope.FromTaggedCbor(receivedUr.Cbor)
            .CheckEncoding();

        // Bob receives the message, validates Alice's signature, and reads
        // the message.
        var verifiedEnvelope = receivedEnvelope
            .VerifySignatureFrom(aliceSshPublicKey);
        var receivedPlaintext = verifiedEnvelope.ExtractSubject<string>();
        Assert.Equal("Hello.", receivedPlaintext);

        // Confirm that it wasn't signed by Carol.
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.VerifySignatureFrom(carolSshPublicKey));

        // Confirm that it was signed by Alice OR Carol.
        receivedEnvelope.VerifySignaturesFromThreshold(
            new IVerifier[] { aliceSshPublicKey, carolSshPublicKey },
            1);

        // Confirm that it was not signed by Alice AND Carol.
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.VerifySignaturesFromThreshold(
                new IVerifier[] { aliceSshPublicKey, carolSshPublicKey },
                2));
    }
}
