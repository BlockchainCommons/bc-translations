using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class EncryptedTests
{
    private static Envelope BasicEnvelope() => Envelope.Create("Hello.");

    private static Envelope KnownValueEnvelope() => Envelope.Create(KnownValuesRegistry.Note);

    private static Envelope AssertionEnvelope() => Envelope.CreateAssertion("knows", "Bob");

    private static Envelope SingleAssertionEnvelope() =>
        Envelope.Create("Alice").AddAssertion("knows", "Bob");

    private static Envelope DoubleAssertionEnvelope() =>
        SingleAssertionEnvelope().AddAssertion("knows", "Carol");

    private static Envelope WrappedEnvelope() => BasicEnvelope().Wrap();

    private static Envelope DoubleWrappedEnvelope() => WrappedEnvelope().Wrap();

    private static SymmetricKey TestSymmetricKey() =>
        SymmetricKey.FromData(Convert.FromHexString(
            "38900719dea655e9a1bc1682aaccf0bfcd79a7239db672d39216e4acdd660dc0"));

    private static Nonce FakeNonce() =>
        Nonce.FromData(Convert.FromHexString("4d785658f36c22fb5aed3ac0"));

    private static void EncryptedTest(Envelope e1)
    {
        var e2 = e1
            .EncryptSubject(TestSymmetricKey(), FakeNonce())
            .CheckEncoding();

        Assert.True(e1.IsEquivalentTo(e2));
        Assert.True(e1.Subject.IsEquivalentTo(e2.Subject));

        var encryptedMessage = e2.ExtractSubject<EncryptedMessage>();
        Assert.Equal(e1.Subject.GetDigest(), ((IDigestProvider)encryptedMessage).GetDigest());

        var e3 = e2.DecryptSubject(TestSymmetricKey());

        Assert.True(e1.IsEquivalentTo(e3));
    }

    [Fact]
    public void TestEncrypted()
    {
        EncryptedTest(BasicEnvelope());
        EncryptedTest(WrappedEnvelope());
        EncryptedTest(DoubleWrappedEnvelope());
        EncryptedTest(KnownValueEnvelope());
        EncryptedTest(AssertionEnvelope());
        EncryptedTest(SingleAssertionEnvelope());
        EncryptedTest(DoubleAssertionEnvelope());
    }
}
