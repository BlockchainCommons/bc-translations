using BlockchainCommons.BCComponents;
using BlockchainCommons.KnownValues;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Public key encryption extension for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Provides methods for encrypting envelopes to one or more recipients
/// using key encapsulation, and for decrypting envelopes as a recipient.
/// Each recipient gets their own sealed message containing the content key
/// encrypted to their public key.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Returns a new envelope with an added <c>hasRecipient: SealedMessage</c> assertion.
    /// </summary>
    /// <param name="recipient">The public keys of the recipient.</param>
    /// <param name="contentKey">The symmetric key used to encrypt the envelope's subject.</param>
    /// <returns>A new envelope with the recipient assertion added.</returns>
    public Envelope AddRecipient(IEncrypter recipient, SymmetricKey contentKey)
    {
        return AddRecipientOpt(recipient, contentKey, null);
    }

    /// <summary>
    /// Adds a <c>hasRecipient</c> assertion with an optional test nonce for deterministic testing.
    /// </summary>
    /// <param name="recipient">The public keys of the recipient.</param>
    /// <param name="contentKey">The symmetric key used to encrypt the envelope's subject.</param>
    /// <param name="testNonce">Optional nonce for deterministic encryption (testing only).</param>
    /// <returns>A new envelope with the recipient assertion added.</returns>
    internal Envelope AddRecipientOpt(IEncrypter recipient, SymmetricKey contentKey, Nonce? testNonce)
    {
        var assertion = MakeHasRecipient(recipient, contentKey, testNonce);
        return AddAssertionEnvelope(assertion);
    }

    /// <summary>
    /// Returns all <see cref="SealedMessage"/> objects from the envelope's
    /// <c>hasRecipient</c> assertions.
    /// </summary>
    /// <returns>A list of sealed messages, one for each recipient.</returns>
    public List<SealedMessage> Recipients()
    {
        var result = new List<SealedMessage>();
        foreach (var assertion in AssertionsWithPredicate(KnownValuesRegistry.HasRecipient))
        {
            var obj = assertion.AsObject()!;
            if (obj.IsObscured)
                continue;
            result.Add(obj.ExtractSubject<SealedMessage>());
        }
        return result;
    }

    /// <summary>
    /// Encrypts the envelope's subject and adds recipient assertions for multiple recipients.
    /// </summary>
    /// <param name="recipients">The public keys of the recipients.</param>
    /// <returns>A new envelope with encrypted subject and recipient assertions.</returns>
    public Envelope EncryptSubjectToRecipients(IReadOnlyList<IEncrypter> recipients)
    {
        return EncryptSubjectToRecipientsOpt(recipients, null);
    }

    /// <summary>
    /// Encrypts the envelope's subject to multiple recipients with an optional test nonce.
    /// </summary>
    /// <param name="recipients">The public keys of the recipients.</param>
    /// <param name="testNonce">Optional nonce for deterministic encryption (testing only).</param>
    /// <returns>A new envelope with encrypted subject and recipient assertions.</returns>
    internal Envelope EncryptSubjectToRecipientsOpt(IReadOnlyList<IEncrypter> recipients, Nonce? testNonce)
    {
        var contentKey = SymmetricKey.New();
        var e = EncryptSubject(contentKey);
        foreach (var recipient in recipients)
        {
            e = e.AddRecipientOpt(recipient, contentKey, testNonce);
        }
        return e;
    }

    /// <summary>
    /// Encrypts the envelope's subject and adds a recipient assertion for a single recipient.
    /// </summary>
    /// <param name="recipient">The public keys of the recipient.</param>
    /// <returns>A new envelope with encrypted subject and recipient assertion.</returns>
    public Envelope EncryptSubjectToRecipient(IEncrypter recipient)
    {
        return EncryptSubjectToRecipientOpt(recipient, null);
    }

    /// <summary>
    /// Encrypts the envelope's subject to a single recipient with an optional test nonce.
    /// </summary>
    /// <param name="recipient">The public keys of the recipient.</param>
    /// <param name="testNonce">Optional nonce for deterministic encryption (testing only).</param>
    /// <returns>A new envelope with encrypted subject and recipient assertion.</returns>
    internal Envelope EncryptSubjectToRecipientOpt(IEncrypter recipient, Nonce? testNonce)
    {
        return EncryptSubjectToRecipientsOpt(new[] { recipient }, testNonce);
    }

    /// <summary>
    /// Decrypts an envelope's subject using the recipient's private key.
    /// </summary>
    /// <remarks>
    /// Finds and extracts all sealed messages from <c>hasRecipient</c> assertions,
    /// tries to decrypt each one with the provided private key, extracts the content
    /// key, and uses it to decrypt the envelope's subject.
    /// </remarks>
    /// <param name="recipient">The private key of the recipient.</param>
    /// <returns>A new envelope with decrypted subject.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if no sealed message can be decrypted with the provided key.
    /// </exception>
    public Envelope DecryptSubjectToRecipient(IDecrypter recipient)
    {
        var sealedMessages = Recipients();
        var contentKeyData = FirstPlaintextInSealedMessages(sealedMessages, recipient);
        var contentKey = SymmetricKey.FromTaggedCbor(Cbor.TryFromData(contentKeyData));
        return DecryptSubject(contentKey);
    }

    /// <summary>
    /// Wraps and encrypts an envelope to a single recipient.
    /// </summary>
    /// <param name="recipient">The public keys of the recipient.</param>
    /// <returns>A new envelope that wraps and encrypts the original to the recipient.</returns>
    public Envelope EncryptToRecipient(IEncrypter recipient)
    {
        return Wrap().EncryptSubjectToRecipient(recipient);
    }

    /// <summary>
    /// Decrypts an envelope that was encrypted to a recipient and unwraps it.
    /// </summary>
    /// <param name="recipient">The private key of the recipient.</param>
    /// <returns>The original, unwrapped envelope.</returns>
    public Envelope DecryptToRecipient(IDecrypter recipient)
    {
        return DecryptSubjectToRecipient(recipient).TryUnwrap();
    }

    // --- Private helpers ---

    private static Envelope MakeHasRecipient(
        IEncrypter recipient,
        SymmetricKey contentKey,
        Nonce? testNonce)
    {
        var sealedMessage = SealedMessage.CreateOpt(
            contentKey.TaggedCbor().ToCborData(),
            recipient,
            null,
            testNonce);
        return CreateAssertion(KnownValuesRegistry.HasRecipient, sealedMessage);
    }

    private static byte[] FirstPlaintextInSealedMessages(
        List<SealedMessage> sealedMessages,
        IDecrypter privateKey)
    {
        foreach (var sealedMessage in sealedMessages)
        {
            try
            {
                return sealedMessage.Decrypt(privateKey);
            }
            catch
            {
                // Try next sealed message
            }
        }
        throw EnvelopeException.UnknownRecipient();
    }
}
