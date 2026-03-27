using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class CryptoTests
{
    private const string PlaintextHello = "Hello.";

    private static Envelope HelloEnvelope() => Envelope.Create(PlaintextHello);

    private static PrivateKeyBase AlicePrivateKey() =>
        PrivateKeyBase.FromData(Convert.FromHexString("82f32c855d3d542256180810797e0073"));

    private static PublicKeys AlicePublicKeys() =>
        ((IPublicKeysProvider)AlicePrivateKey()).PublicKeys();

    private static PrivateKeyBase BobPrivateKey() =>
        PrivateKeyBase.FromData(Convert.FromHexString("187a5973c64d359c836eba466a44db7b"));

    private static PublicKeys BobPublicKeys() =>
        ((IPublicKeysProvider)BobPrivateKey()).PublicKeys();

    private static PrivateKeyBase CarolPrivateKey() =>
        PrivateKeyBase.FromData(Convert.FromHexString("8574afab18e229651c1be8f76ffee523"));

    private static PublicKeys CarolPublicKeys() =>
        ((IPublicKeysProvider)CarolPrivateKey()).PublicKeys();

    /// <summary>
    /// CBOR round-trip helper (substitute for UR round-trip since Envelope
    /// does not yet implement IURCodable in C#).
    /// </summary>
    private static Envelope CborRoundTrip(Envelope envelope)
    {
        var data = envelope.TaggedCbor().ToCborData();
        return Envelope.FromCborData(data).CheckEncoding();
    }

    [Fact]
    public void TestPlaintext()
    {
        GlobalFormatContext.RegisterTags();

        // Alice sends a plaintext message to Bob.
        var envelope = HelloEnvelope();

        Assert.Equal("\"Hello.\"", envelope.Format());

        // Bob receives the envelope and reads the message.
        var receivedPlaintext = CborRoundTrip(envelope)
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, receivedPlaintext);
    }

    [Fact]
    public void TestSymmetricEncryption()
    {
        GlobalFormatContext.RegisterTags();

        // Alice and Bob have agreed to use this key.
        var key = SymmetricKey.New();

        // Alice sends a message encrypted with the key to Bob.
        var envelope = HelloEnvelope()
            .EncryptSubject(key)
            .CheckEncoding();

        Assert.Equal("ENCRYPTED", envelope.Format());

        // Bob receives the envelope.
        var receivedEnvelope = CborRoundTrip(envelope);

        // Bob decrypts and reads the message.
        var receivedPlaintext = receivedEnvelope
            .DecryptSubject(key)
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, receivedPlaintext);

        // Can't read with no key.
        Assert.Throws<EnvelopeException>(() =>
            receivedEnvelope.ExtractSubject<string>());

        // Can't read with incorrect key.
        Assert.ThrowsAny<Exception>(() =>
            receivedEnvelope.DecryptSubject(SymmetricKey.New()));
    }

    private static void RoundTripTest(Envelope envelope)
    {
        var key = SymmetricKey.New();
        var plaintextSubject = envelope.CheckEncoding();
        var encryptedSubject = plaintextSubject.EncryptSubject(key);
        Assert.True(encryptedSubject.IsEquivalentTo(plaintextSubject));
        var plaintextSubject2 = encryptedSubject
            .DecryptSubject(key)
            .CheckEncoding();
        Assert.True(encryptedSubject.IsEquivalentTo(plaintextSubject2));
        Assert.True(plaintextSubject.IsIdenticalTo(plaintextSubject2));
    }

    [Fact]
    public void TestEncryptDecrypt()
    {
        // leaf
        RoundTripTest(Envelope.Create(PlaintextHello));

        // node
        RoundTripTest(Envelope.Create("Alice").AddAssertion("knows", "Bob"));

        // wrapped
        RoundTripTest(Envelope.Create("Alice").Wrap());

        // known value
        RoundTripTest(Envelope.Create(KnownValuesRegistry.IsA));

        // assertion
        RoundTripTest(Envelope.CreateAssertion("knows", "Bob"));

        // compressed
        RoundTripTest(Envelope.Create(PlaintextHello).Compress());
    }

    [Fact]
    public void TestSignThenEncrypt()
    {
        GlobalFormatContext.RegisterTags();

        // Alice and Bob have agreed to use this key.
        var key = SymmetricKey.New();

        // Alice signs a plaintext message, then encrypts it.
        var envelope = HelloEnvelope()
            .AddSignature(AlicePrivateKey())
            .CheckEncoding()
            .Wrap()
            .CheckEncoding()
            .EncryptSubject(key)
            .CheckEncoding();

        Assert.Equal("ENCRYPTED", envelope.Format());

        // Bob receives the envelope, decrypts it using the shared key, and then
        // validates Alice's signature.
        var receivedPlaintext = CborRoundTrip(envelope)
            .DecryptSubject(key)
            .CheckEncoding()
            .TryUnwrap()
            .CheckEncoding()
            .VerifySignatureFrom(AlicePublicKeys())
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, receivedPlaintext);
    }

    [Fact]
    public void TestEncryptThenSign()
    {
        GlobalFormatContext.RegisterTags();

        // Alice and Bob have agreed to use this key.
        var key = SymmetricKey.New();

        // Alice encrypts a plaintext message, then signs it.
        var envelope = HelloEnvelope()
            .EncryptSubject(key)
            .AddSignature(AlicePrivateKey())
            .CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'signed': Signature\n" +
            "]",
            envelope.Format());

        // Bob receives the envelope, validates Alice's signature, then decrypts
        // the message.
        var receivedPlaintext = CborRoundTrip(envelope)
            .VerifySignatureFrom(AlicePublicKeys())
            .DecryptSubject(key)
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, receivedPlaintext);
    }

    [Fact]
    public void TestMultiRecipient()
    {
        // Alice encrypts a message so that it can only be decrypted by Bob or
        // Carol.
        var contentKey = SymmetricKey.New();
        var envelope = HelloEnvelope()
            .EncryptSubject(contentKey)
            .AddRecipient(BobPublicKeys(), contentKey)
            .AddRecipient(CarolPublicKeys(), contentKey)
            .CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasRecipient': SealedMessage\n" +
            "]",
            envelope.Format());

        // The envelope is received
        var receivedEnvelope = CborRoundTrip(envelope);

        // Bob decrypts and reads the message
        var bobReceivedPlaintext = receivedEnvelope
            .DecryptSubjectToRecipient(BobPrivateKey())
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, bobReceivedPlaintext);

        // Carol decrypts and reads the message
        var carolReceivedPlaintext = receivedEnvelope
            .DecryptSubjectToRecipient(CarolPrivateKey())
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, carolReceivedPlaintext);

        // Alice didn't encrypt it to herself, so she can't read it.
        Assert.ThrowsAny<Exception>(() =>
            receivedEnvelope.DecryptSubjectToRecipient(AlicePrivateKey()));
    }

    [Fact]
    public void TestVisibleSignatureMultiRecipient()
    {
        // Alice signs a message, and then encrypts it so that it can only be
        // decrypted by Bob or Carol.
        var contentKey = SymmetricKey.New();
        var envelope = HelloEnvelope()
            .AddSignature(AlicePrivateKey())
            .EncryptSubject(contentKey)
            .AddRecipient(BobPublicKeys(), contentKey)
            .AddRecipient(CarolPublicKeys(), contentKey)
            .CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'signed': Signature\n" +
            "]",
            envelope.Format());

        // The envelope is received
        var receivedEnvelope = CborRoundTrip(envelope);

        // Bob validates Alice's signature, then decrypts and reads the message
        var bobReceivedPlaintext = receivedEnvelope
            .VerifySignatureFrom(AlicePublicKeys())
            .DecryptSubjectToRecipient(BobPrivateKey())
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, bobReceivedPlaintext);

        // Carol validates Alice's signature, then decrypts and reads the message
        var carolReceivedPlaintext = receivedEnvelope
            .VerifySignatureFrom(AlicePublicKeys())
            .DecryptSubjectToRecipient(CarolPrivateKey())
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, carolReceivedPlaintext);

        // Alice didn't encrypt it to herself, so she can't read it.
        Assert.ThrowsAny<Exception>(() =>
            receivedEnvelope.DecryptSubjectToRecipient(AlicePrivateKey()));
    }

    [Fact]
    public void TestHiddenSignatureMultiRecipient()
    {
        // Alice signs a message, and then encloses it in another envelope before
        // encrypting it so that it can only be decrypted by Bob or Carol. This
        // hides Alice's signature, and requires recipients to decrypt the
        // subject before they are able to validate the signature.
        var contentKey = SymmetricKey.New();
        var envelope = HelloEnvelope()
            .AddSignature(AlicePrivateKey())
            .Wrap()
            .EncryptSubject(contentKey)
            .AddRecipient(BobPublicKeys(), contentKey)
            .AddRecipient(CarolPublicKeys(), contentKey)
            .CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasRecipient': SealedMessage\n" +
            "]",
            envelope.Format());

        // The envelope is received
        var receivedEnvelope = CborRoundTrip(envelope);

        // Bob decrypts the envelope, then extracts the inner envelope and validates
        // Alice's signature, then reads the message
        var bobReceivedPlaintext = receivedEnvelope
            .DecryptSubjectToRecipient(BobPrivateKey())
            .TryUnwrap()
            .CheckEncoding()
            .VerifySignatureFrom(AlicePublicKeys())
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, bobReceivedPlaintext);

        // Carol decrypts the envelope, then extracts the inner envelope and
        // validates Alice's signature, then reads the message
        var carolReceivedPlaintext = receivedEnvelope
            .DecryptSubjectToRecipient(CarolPrivateKey())
            .TryUnwrap()
            .CheckEncoding()
            .VerifySignatureFrom(AlicePublicKeys())
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, carolReceivedPlaintext);

        // Alice didn't encrypt it to herself, so she can't read it.
        Assert.ThrowsAny<Exception>(() =>
            receivedEnvelope.DecryptSubjectToRecipient(AlicePrivateKey()));
    }

    [Fact]
    public void TestSecret1()
    {
        GlobalFormatContext.RegisterTags();
        var bobPassword = Encoding.UTF8.GetBytes("correct horse battery staple");

        // Alice encrypts a message so that it can only be decrypted by Bob's password.
        var envelope = HelloEnvelope()
            .Lock(KeyDerivationMethod.HKDF, bobPassword);
        envelope.CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'hasSecret': EncryptedKey(HKDF(SHA256))\n" +
            "]",
            envelope.Format());

        // The envelope is received
        var receivedEnvelope = CborRoundTrip(envelope);

        // Bob decrypts and reads the message
        var bobReceivedPlaintext = receivedEnvelope
            .Unlock(bobPassword)
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, bobReceivedPlaintext);

        // Eve tries to decrypt the message with a different password
        Assert.ThrowsAny<Exception>(() =>
            receivedEnvelope.Unlock(Encoding.UTF8.GetBytes("wrong password")));
    }

    [Fact]
    public void TestSecret2()
    {
        GlobalFormatContext.RegisterTags();

        // Alice encrypts a message so that it can be decrypted by three specific
        // passwords.
        var bobPassword = Encoding.UTF8.GetBytes("correct horse battery staple");
        var carolPassword = Encoding.UTF8.GetBytes("Able was I ere I saw Elba");
        var gracyPassword = Encoding.UTF8.GetBytes("Madam, in Eden, I'm Adam");
        var contentKey = SymmetricKey.New();
        var envelope = HelloEnvelope()
            .EncryptSubject(contentKey)
            .AddSecret(KeyDerivationMethod.HKDF, bobPassword, contentKey)
            .AddSecret(KeyDerivationMethod.Scrypt, carolPassword, contentKey)
            .AddSecret(KeyDerivationMethod.Argon2id, gracyPassword, contentKey)
            .CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'hasSecret': EncryptedKey(Argon2id)\n" +
            "    'hasSecret': EncryptedKey(HKDF(SHA256))\n" +
            "    'hasSecret': EncryptedKey(Scrypt)\n" +
            "]",
            envelope.Format());

        // The envelope is received
        var receivedEnvelope = CborRoundTrip(envelope);

        // Bob decrypts and reads the message
        var bobReceivedPlaintext = receivedEnvelope
            .UnlockSubject(bobPassword)
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, bobReceivedPlaintext);

        // Carol decrypts and reads the message
        var carolReceivedPlaintext = receivedEnvelope
            .UnlockSubject(carolPassword)
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, carolReceivedPlaintext);

        // Gracy decrypts and reads the message
        var gracyReceivedPlaintext = receivedEnvelope
            .UnlockSubject(gracyPassword)
            .CheckEncoding()
            .ExtractSubject<string>();
        Assert.Equal(PlaintextHello, gracyReceivedPlaintext);

        // Eve tries to decrypt the message with a different password
        Assert.ThrowsAny<Exception>(() =>
            receivedEnvelope.UnlockSubject(Encoding.UTF8.GetBytes("wrong password")));
    }
}
