using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class Ed25519Tests
{
    [Fact]
    public void TestEd25519SignedPlaintext()
    {
        TagsRegistry.RegisterTags();

        var aliceEd25519PrivateKey = TestData.AlicePrivateKey().Ed25519SigningPrivateKey();
        var aliceSigningPrivateKey = SigningPrivateKey.NewEd25519(aliceEd25519PrivateKey);
        var alicePublicKey = aliceSigningPrivateKey.PublicKey();

        // Alice sends a signed plaintext message to Bob.
        var envelope = TestData.HelloEnvelope()
            .AddSignature(aliceSigningPrivateKey)
            .CheckEncoding();
        var ur = BCUR.UR.Create("envelope", envelope.TaggedCbor());

        var expectedFormat =
            "\"Hello.\" [\n" +
            "    'signed': Signature(Ed25519)\n" +
            "]";
        Assert.Equal(expectedFormat, envelope.Format());

        // Bob receives the envelope.
        var receivedUr = BCUR.UR.FromUrString(ur.ToUrString());
        var receivedEnvelope = Envelope.FromTaggedCbor(receivedUr.Cbor)
            .CheckEncoding();

        // Bob receives the message, validates Alice's signature, and reads
        // the message.
        var verifiedEnvelope = receivedEnvelope
            .VerifySignatureFrom(alicePublicKey);
        var receivedPlaintext = verifiedEnvelope.ExtractSubject<string>();
        Assert.Equal("Hello.", receivedPlaintext);

        // Confirm that it wasn't signed by Carol.
        var carolEd25519PrivateKey = TestData.CarolPrivateKey().Ed25519SigningPrivateKey();
        var carolSigningPrivateKey = SigningPrivateKey.NewEd25519(carolEd25519PrivateKey);
        var carolPublicKey = carolSigningPrivateKey.PublicKey();
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.VerifySignatureFrom(carolPublicKey));

        // Confirm that it was signed by Alice OR Carol.
        receivedEnvelope.VerifySignaturesFromThreshold(
            new IVerifier[] { alicePublicKey, carolPublicKey },
            1);

        // Confirm that it was not signed by Alice AND Carol.
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.VerifySignaturesFromThreshold(
                new IVerifier[] { alicePublicKey, carolPublicKey },
                2));
    }
}
