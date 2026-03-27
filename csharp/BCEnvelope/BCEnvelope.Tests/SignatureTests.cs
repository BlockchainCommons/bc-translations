using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class SignatureTests
{
    [Fact]
    public void TestSignedPlaintext()
    {
        // Alice sends a signed plaintext message to Bob.
        var envelope = TestData.HelloEnvelope()
            .AddSignature(TestData.AlicePrivateKey())
            .CheckEncoding();

        var ur = BCUR.UR.Create("envelope", envelope.TaggedCbor());

        var expectedFormat =
            "\"Hello.\" [\n" +
            "    'signed': Signature\n" +
            "]";
        Assert.Equal(expectedFormat, envelope.Format());

        // Bob receives the envelope.
        var receivedUr = BCUR.UR.FromUrString(ur.ToUrString());
        var receivedEnvelope = Envelope.FromTaggedCbor(receivedUr.Cbor)
            .CheckEncoding();

        // Bob receives the message, validates Alice's signature, and reads
        // the message.
        var verifiedEnvelope = receivedEnvelope
            .VerifySignatureFrom(TestData.AlicePrivateKey());
        var receivedPlaintext = verifiedEnvelope.ExtractSubject<string>();
        Assert.Equal("Hello.", receivedPlaintext);

        // Confirm that it wasn't signed by Carol.
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.VerifySignatureFrom(TestData.CarolPrivateKey()));

        // Confirm that it was signed by Alice OR Carol.
        receivedEnvelope.VerifySignaturesFromThreshold(
            new IVerifier[] { TestData.AlicePrivateKey(), TestData.CarolPrivateKey() },
            1);

        // Confirm that it was not signed by Alice AND Carol.
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.VerifySignaturesFromThreshold(
                new IVerifier[] { TestData.AlicePrivateKey(), TestData.CarolPrivateKey() },
                2));
    }

    [Fact]
    public void TestMultisignedPlaintext()
    {
        TagsRegistry.RegisterTags();

        // Alice and Carol jointly send a signed plaintext message to Bob.
        var envelope = TestData.HelloEnvelope()
            .AddSignatures(new ISigner[] { TestData.AlicePrivateKey(), TestData.CarolPrivateKey() })
            .CheckEncoding();

        var expectedFormat =
            "\"Hello.\" [\n" +
            "    'signed': Signature\n" +
            "    'signed': Signature\n" +
            "]";
        Assert.Equal(expectedFormat, envelope.Format());

        // Alice & Carol -> Bob
        var ur = BCUR.UR.Create("envelope", envelope.TaggedCbor());

        // Bob receives the envelope and verifies both signatures.
        var receivedUr = BCUR.UR.FromUrString(ur.ToUrString());
        var receivedEnvelope = Envelope.FromTaggedCbor(receivedUr.Cbor)
            .CheckEncoding()
            .VerifySignaturesFrom(
                new IVerifier[] { TestData.AlicePrivateKey(), TestData.CarolPrivateKey() });

        // Bob reads the message.
        var receivedPlaintext = receivedEnvelope.ExtractSubject<string>();
        Assert.Equal(TestData.PlaintextHello, receivedPlaintext);
    }

    [Fact]
    public void TestSignedWithMetadata()
    {
        TagsRegistry.RegisterTags();

        var envelope = TestData.HelloEnvelope();

        var metadata = new SignatureMetadata()
            .WithAssertion(KnownValuesRegistry.Note, "Alice signed this.");

        var signedEnvelope = envelope
            .Wrap()
            .AddSignatureOpt(TestData.AlicePrivateKey(), null, metadata)
            .CheckEncoding();

        var expectedFormat =
            "{\n" +
            "    \"Hello.\"\n" +
            "} [\n" +
            "    'signed': {\n" +
            "        Signature [\n" +
            "            'note': \"Alice signed this.\"\n" +
            "        ]\n" +
            "    } [\n" +
            "        'signed': Signature\n" +
            "    ]\n" +
            "]";
        Assert.Equal(expectedFormat, signedEnvelope.Format());

        // Alice -> Bob
        var ur = BCUR.UR.Create("envelope", signedEnvelope.TaggedCbor());

        // Bob receives the envelope and verifies the message was signed by Alice.
        var receivedUr = BCUR.UR.FromUrString(ur.ToUrString());
        var receivedEnvelope = Envelope.FromTaggedCbor(receivedUr.Cbor)
            .CheckEncoding();
        var (unwrapped, metadataEnvelope) = receivedEnvelope
            .VerifyReturningMetadata(TestData.AlicePrivateKey());

        var expectedMetadataFormat =
            "Signature [\n" +
            "    'note': \"Alice signed this.\"\n" +
            "]";
        Assert.Equal(expectedMetadataFormat, metadataEnvelope.Format());

        var note = metadataEnvelope
            .ObjectForPredicate(KnownValuesRegistry.Note)
            .ExtractSubject<string>();
        Assert.Equal("Alice signed this.", note);

        // Bob reads the message.
        var receivedPlaintext = unwrapped.ExtractSubject<string>();
        Assert.Equal(TestData.PlaintextHello, receivedPlaintext);
    }
}
