using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Symmetric encryption and decryption operations for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Encryption preserves the envelope's digest, which means signatures,
/// proofs, and other cryptographic artifacts remain valid even when parts
/// of the envelope are encrypted.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Returns a new envelope with its subject encrypted using the given key.
    /// </summary>
    /// <remarks>
    /// Encrypts only the subject of the envelope, leaving assertions
    /// unencrypted. To encrypt an entire envelope including its assertions,
    /// use the <see cref="Encrypt"/> convenience method.
    /// </remarks>
    /// <param name="key">The symmetric key to use for encryption.</param>
    /// <param name="testNonce">
    /// Optional nonce for deterministic testing. If <c>null</c>, a random nonce is generated.
    /// </param>
    /// <returns>A new envelope with its subject encrypted.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if the envelope is already encrypted or elided.
    /// </exception>
    public Envelope EncryptSubject(SymmetricKey key, Nonce? testNonce = null)
    {
        Envelope result;
        Digest originalDigest;

        switch (Case)
        {
            case EnvelopeCase.NodeCase node:
            {
                if (node.Subject.IsEncrypted)
                    throw EnvelopeException.AlreadyEncrypted();
                var encodedCbor = node.Subject.TaggedCbor().ToCborData();
                var digest = node.Subject.GetDigest();
                var message = key.EncryptWithDigest(encodedCbor, digest, testNonce);
                var encryptedSubject = CreateWithEncrypted(message);
                result = CreateWithUncheckedAssertions(encryptedSubject, node.Assertions.ToList());
                originalDigest = node.Digest;
                break;
            }

            case EnvelopeCase.LeafCase leaf:
            {
                var encodedCbor = TaggedCbor().ToCborData();
                var message = key.EncryptWithDigest(encodedCbor, leaf.Digest, testNonce);
                result = CreateWithEncrypted(message);
                originalDigest = leaf.Digest;
                break;
            }

            case EnvelopeCase.WrappedCase wrapped:
            {
                var encodedCbor = TaggedCbor().ToCborData();
                var message = key.EncryptWithDigest(encodedCbor, wrapped.Digest, testNonce);
                result = CreateWithEncrypted(message);
                originalDigest = wrapped.Digest;
                break;
            }

            case EnvelopeCase.KnownValueCase kv:
            {
                var encodedCbor = TaggedCbor().ToCborData();
                var message = key.EncryptWithDigest(encodedCbor, kv.Digest, testNonce);
                result = CreateWithEncrypted(message);
                originalDigest = kv.Digest;
                break;
            }

            case EnvelopeCase.AssertionCase assertionCase:
            {
                var digest = assertionCase.Assertion.GetDigest();
                var encodedCbor = TaggedCbor().ToCborData();
                var message = key.EncryptWithDigest(encodedCbor, digest, testNonce);
                result = CreateWithEncrypted(message);
                originalDigest = digest;
                break;
            }

            case EnvelopeCase.EncryptedCase:
                throw EnvelopeException.AlreadyEncrypted();

            case EnvelopeCase.CompressedCase compressed:
            {
                var digest = compressed.Compressed.Digest
                    ?? throw EnvelopeException.MissingDigest();
                var encodedCbor = TaggedCbor().ToCborData();
                var message = key.EncryptWithDigest(encodedCbor, digest, testNonce);
                result = CreateWithEncrypted(message);
                originalDigest = digest;
                break;
            }

            case EnvelopeCase.ElidedCase:
                throw EnvelopeException.AlreadyElided();

            default:
                throw new InvalidOperationException("Unknown envelope case");
        }

        if (result.GetDigest() != originalDigest)
            throw EnvelopeException.InvalidDigest();

        return result;
    }

    /// <summary>
    /// Returns a new envelope with its subject decrypted using the given key.
    /// </summary>
    /// <param name="key">The symmetric key to use for decryption.</param>
    /// <returns>A new envelope with its subject decrypted.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if the subject is not encrypted, the key is incorrect, or the
    /// decrypted digest does not match.
    /// </exception>
    public Envelope DecryptSubject(SymmetricKey key)
    {
        if (Subject.Case is not EnvelopeCase.EncryptedCase encrypted)
            throw EnvelopeException.NotEncrypted();

        var decryptedData = key.Decrypt(encrypted.EncryptedMessage);
        var subjectDigest = encrypted.EncryptedMessage.AadDigest()
            ?? throw EnvelopeException.MissingDigest();

        var cbor = Cbor.TryFromData(decryptedData);
        var resultSubject = FromTaggedCbor(cbor);

        if (resultSubject.GetDigest() != subjectDigest)
            throw EnvelopeException.InvalidDigest();

        if (Case is EnvelopeCase.NodeCase node)
        {
            var result = CreateWithUncheckedAssertions(resultSubject, node.Assertions.ToList());
            if (result.GetDigest() != node.Digest)
                throw EnvelopeException.InvalidDigest();
            return result;
        }

        return resultSubject;
    }

    /// <summary>
    /// Convenience method to encrypt an entire envelope including its assertions.
    /// </summary>
    /// <remarks>
    /// This method wraps the envelope and then encrypts its subject, which has
    /// the effect of encrypting the entire original envelope.
    /// </remarks>
    /// <param name="key">The symmetric key to use for encryption.</param>
    /// <param name="testNonce">Optional nonce for deterministic testing.</param>
    /// <returns>A new envelope with the entire original envelope encrypted.</returns>
    public Envelope Encrypt(SymmetricKey key, Nonce? testNonce = null)
        => Wrap().EncryptSubject(key, testNonce);

    /// <summary>
    /// Convenience method to decrypt an entire envelope that was encrypted
    /// using <see cref="Encrypt"/>.
    /// </summary>
    /// <param name="key">The symmetric key to use for decryption.</param>
    /// <returns>The original decrypted envelope.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if decryption or unwrapping fails.
    /// </exception>
    public Envelope Decrypt(SymmetricKey key)
        => DecryptSubject(key).TryUnwrap();
}
