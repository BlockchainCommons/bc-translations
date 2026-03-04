using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A sealed message that can only be decrypted by the intended recipient.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SealedMessage"/> provides a public key encryption mechanism where
/// a message is encrypted with a symmetric key, and that key is then encapsulated
/// using the recipient's public key. This ensures that only the recipient can
/// decrypt the message by first decapsulating the shared secret using their
/// private key.
/// </para>
/// <para>
/// Features:
/// <list type="bullet">
/// <item>Anonymous sender: The sender's identity is not revealed</item>
/// <item>Authenticated encryption: Message integrity and authenticity are guaranteed</item>
/// <item>Forward secrecy: Each message uses a different ephemeral key</item>
/// <item>Post-quantum security options: Can use ML-KEM for quantum-resistant encryption</item>
/// </list>
/// </para>
/// <para>
/// CDDL:
/// <code>
/// SealedMessage = #6.40019([ encrypted_message, encapsulated_key ])
/// </code>
/// </para>
/// </remarks>
public sealed class SealedMessage : IEquatable<SealedMessage>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>Gets the encrypted message content.</summary>
    public EncryptedMessage Message { get; }

    /// <summary>Gets the encapsulated key used to encrypt the message.</summary>
    public EncapsulationCiphertext EncapsulatedKey { get; }

    /// <summary>
    /// Creates a new <see cref="SealedMessage"/> from its component parts.
    /// </summary>
    /// <param name="message">The encrypted message.</param>
    /// <param name="encapsulatedKey">The encapsulated key.</param>
    public SealedMessage(EncryptedMessage message, EncapsulationCiphertext encapsulatedKey)
    {
        Message = message;
        EncapsulatedKey = encapsulatedKey;
    }

    /// <summary>
    /// Creates a new <see cref="SealedMessage"/> by encrypting plaintext for the
    /// given recipient.
    /// </summary>
    /// <param name="plaintext">The message data to encrypt.</param>
    /// <param name="recipient">The recipient who will be able to decrypt the message.</param>
    /// <returns>A new <see cref="SealedMessage"/>.</returns>
    public static SealedMessage Create(byte[] plaintext, IEncrypter recipient)
    {
        return CreateWithAad(plaintext, recipient, null);
    }

    /// <summary>
    /// Creates a new <see cref="SealedMessage"/> with additional authenticated data (AAD).
    /// </summary>
    /// <param name="plaintext">The message data to encrypt.</param>
    /// <param name="recipient">The recipient who will be able to decrypt the message.</param>
    /// <param name="aad">Optional additional authenticated data.</param>
    /// <returns>A new <see cref="SealedMessage"/>.</returns>
    public static SealedMessage CreateWithAad(byte[] plaintext, IEncrypter recipient, byte[]? aad)
    {
        return CreateOpt(plaintext, recipient, aad, null);
    }

    /// <summary>
    /// Creates a new <see cref="SealedMessage"/> with options for testing.
    /// </summary>
    /// <param name="plaintext">The message data to encrypt.</param>
    /// <param name="recipient">The recipient who will be able to decrypt the message.</param>
    /// <param name="aad">Optional additional authenticated data.</param>
    /// <param name="testNonce">Optional nonce for deterministic encryption (testing only).</param>
    /// <returns>A new <see cref="SealedMessage"/>.</returns>
    public static SealedMessage CreateOpt(
        byte[] plaintext,
        IEncrypter recipient,
        byte[]? aad,
        Nonce? testNonce)
    {
        var (sharedKey, encapsulatedKey) = recipient.EncapsulateNewSharedSecret();
        var message = sharedKey.Encrypt(plaintext, aad, testNonce);
        return new SealedMessage(message, encapsulatedKey);
    }

    /// <summary>
    /// Decrypts the message using the recipient's private key.
    /// </summary>
    /// <param name="privateKey">The private key of the intended recipient.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the private key does not match, decapsulation fails, or
    /// decryption fails (e.g., message tampering).
    /// </exception>
    public byte[] Decrypt(IDecrypter privateKey)
    {
        var sharedKey = privateKey.DecapsulateSharedSecret(EncapsulatedKey);
        return sharedKey.Decrypt(Message);
    }

    /// <summary>Gets the encapsulation scheme used for this sealed message.</summary>
    public EncapsulationScheme Scheme => EncapsulatedKey.Scheme;

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the SealedMessage type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagSealedMessage);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as a two-element array
    /// [encrypted_message, encapsulated_key].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Message.TaggedCbor(),
            EncapsulatedKey.ToCbor(),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="SealedMessage"/> from an untagged CBOR array.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be a two-element array).</param>
    /// <returns>A new <see cref="SealedMessage"/>.</returns>
    public static SealedMessage FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("SealedMessage", $"must have two elements, got {elements.Count}");

        var message = EncryptedMessage.FromTaggedCbor(elements[0]);
        var encapsulatedKey = EncapsulationCiphertext.FromCbor(elements[1]);
        return new SealedMessage(message, encapsulatedKey);
    }

    /// <summary>
    /// Decodes a <see cref="SealedMessage"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="SealedMessage"/>.</returns>
    public static SealedMessage FromTaggedCbor(Cbor cbor)
    {
        foreach (var tag in CborTags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException) { }
            catch (CborWrongTypeException) { }
        }
        throw new CborWrongTypeException();
    }

    // --- IEquatable<SealedMessage> ---

    /// <inheritdoc/>
    public bool Equals(SealedMessage? other)
    {
        if (other is null) return false;
        return Message == other.Message
            && EncapsulatedKey == other.EncapsulatedKey;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SealedMessage m && Equals(m);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Message, EncapsulatedKey);

    /// <summary>Tests equality of two SealedMessage instances.</summary>
    public static bool operator ==(SealedMessage? left, SealedMessage? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two SealedMessage instances.</summary>
    public static bool operator !=(SealedMessage? left, SealedMessage? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() =>
        $"SealedMessage(scheme={Scheme}, message={Message})";
}
