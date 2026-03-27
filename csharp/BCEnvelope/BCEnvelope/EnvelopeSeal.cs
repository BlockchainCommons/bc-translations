using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Seal/unseal extension for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Provides convenience methods that combine signing and recipient-based
/// encryption into a single operation. Sealing signs the envelope with the
/// sender's private key and then encrypts it to the recipient's public key.
/// Unsealing reverses these operations.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Seals an envelope by signing it with the sender's key and then
    /// encrypting it to the recipient.
    /// </summary>
    /// <param name="sender">The private key used to sign the envelope.</param>
    /// <param name="recipient">The public key used to encrypt the envelope.</param>
    /// <returns>A new envelope that has been signed and encrypted.</returns>
    public Envelope Seal(ISigner sender, IEncrypter recipient)
    {
        return Sign(sender).EncryptToRecipient(recipient);
    }

    /// <summary>
    /// Seals an envelope with optional signing options.
    /// </summary>
    /// <param name="sender">The private key used to sign the envelope.</param>
    /// <param name="recipient">The public key used to encrypt the envelope.</param>
    /// <param name="options">Optional signing options to control how the signature is created.</param>
    /// <returns>A new envelope that has been signed and encrypted.</returns>
    public Envelope SealOpt(ISigner sender, IEncrypter recipient, SigningOptions? options = null)
    {
        return SignOpt(sender, options).EncryptToRecipient(recipient);
    }

    /// <summary>
    /// Unseals an envelope by decrypting it with the recipient's private key
    /// and then verifying the signature using the sender's public key.
    /// </summary>
    /// <param name="sender">The public key used to verify the signature.</param>
    /// <param name="recipient">The private key used to decrypt the envelope.</param>
    /// <returns>The unsealed envelope.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if decryption or signature verification fails.
    /// </exception>
    public Envelope Unseal(IVerifier sender, IDecrypter recipient)
    {
        return DecryptToRecipient(recipient).Verify(sender);
    }
}
