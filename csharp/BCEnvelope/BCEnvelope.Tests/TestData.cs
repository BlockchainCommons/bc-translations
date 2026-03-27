using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCRand;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Test data factories for constructing common test envelopes.
/// All functions must produce byte-identical results across languages.
/// </summary>
public static class TestData
{
    public const string PlaintextHello = "Hello.";

    public static Envelope HelloEnvelope() => Envelope.Create(PlaintextHello);

    public static Envelope KnownValueEnvelope() => Envelope.Create(KnownValuesRegistry.Note);

    public static Envelope AssertionEnvelope() =>
        Envelope.CreateAssertion("knows", "Bob");

    public static Envelope SingleAssertionEnvelope() =>
        Envelope.Create("Alice").AddAssertion("knows", "Bob");

    public static Envelope DoubleAssertionEnvelope() =>
        SingleAssertionEnvelope().AddAssertion("knows", "Carol");

    public static Envelope WrappedEnvelope() => HelloEnvelope().Wrap();

    public static Envelope DoubleWrappedEnvelope() => WrappedEnvelope().Wrap();

    // ===== Key Material =====

    public static byte[] AliceSeed() =>
        Convert.FromHexString("82f32c855d3d542256180810797e0073");

    public static PrivateKeyBase AlicePrivateKey() =>
        PrivateKeyBase.FromData(AliceSeed());

    public static byte[] BobSeed() =>
        Convert.FromHexString("187a5973c64d359c836eba466a44db7b");

    public static PrivateKeyBase BobPrivateKey() =>
        PrivateKeyBase.FromData(BobSeed());

    public static byte[] CarolSeed() =>
        Convert.FromHexString("8574afab18e229651c1be8f76ffee523");

    public static PrivateKeyBase CarolPrivateKey() =>
        PrivateKeyBase.FromData(CarolSeed());

    // ===== Fake Crypto =====

    public static SymmetricKey FakeContentKey() =>
        SymmetricKey.FromData(Convert.FromHexString(
            "526afd95b2229c5381baec4a1788507a3c4a566ca5cce64543b46ad12aff0035"));

    public static Nonce FakeNonce() =>
        Nonce.FromData(Convert.FromHexString("4d785658f36c22fb5aed3ac0"));

    // ===== Credential =====

    public static Envelope Credential()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var options = new SigningOptions.SchnorrOptions(rng);

        var envelope = Envelope.Create(ARID.FromData(Convert.FromHexString(
                "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d")))
            .AddAssertion(KnownValuesRegistry.IsA, "Certificate of Completion")
            .AddAssertion(KnownValuesRegistry.Issuer, "Example Electrical Engineering Board")
            .AddAssertion(KnownValuesRegistry.Controller, "Example Electrical Engineering Board")
            .AddAssertion("firstName", "James")
            .AddAssertion("lastName", "Maxwell")
            .AddAssertion("issueDate", CborDate.FromYmd(2020, 1, 1))
            .AddAssertion("expirationDate", CborDate.FromYmd(2028, 1, 1))
            .AddAssertion("photo", "This is James Maxwell's photo.")
            .AddAssertion("certificateNumber", "123-456-789")
            .AddAssertion("subject", "RF and Microwave Engineering")
            .AddAssertion("continuingEducationUnits", 1)
            .AddAssertion("professionalDevelopmentHours", 15)
            .AddAssertion("topics", new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromString("Subject 1"),
                Cbor.FromString("Subject 2"),
            })))
            .Wrap()
            .AddSignatureOpt(AlicePrivateKey(), options, null)
            .AddAssertion(KnownValuesRegistry.Note,
                "Signed by Example Electrical Engineering Board");

        return envelope.CheckEncoding();
    }

    public static Envelope RedactedCredential()
    {
        var credential = Credential();
        var target = new HashSet<Digest>();
        target.Add(credential.GetDigest());

        foreach (var assertion in credential.Assertions)
        {
            foreach (var d in assertion.DeepDigests())
                target.Add(d);
        }

        target.Add(credential.Subject.GetDigest());
        var content = credential.Subject.TryUnwrap();
        target.Add(content.GetDigest());
        target.Add(content.Subject.GetDigest());

        foreach (var d in content.AssertionWithPredicate("firstName").ShallowDigests())
            target.Add(d);
        foreach (var d in content.AssertionWithPredicate("lastName").ShallowDigests())
            target.Add(d);
        foreach (var d in content.AssertionWithPredicate(KnownValuesRegistry.IsA).ShallowDigests())
            target.Add(d);
        foreach (var d in content.AssertionWithPredicate(KnownValuesRegistry.Issuer).ShallowDigests())
            target.Add(d);
        foreach (var d in content.AssertionWithPredicate("subject").ShallowDigests())
            target.Add(d);
        foreach (var d in content.AssertionWithPredicate("expirationDate").ShallowDigests())
            target.Add(d);

        return credential.ElideRevealingSet(target);
    }
}
