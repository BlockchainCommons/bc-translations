using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A container combining signing and encapsulation private keys.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PrivateKeys"/> packages a <see cref="SigningPrivateKey"/> for creating
/// digital signatures with an <see cref="EncapsulationPrivateKey"/> for decrypting
/// messages, providing a complete private key set for cryptographic operations.
/// </para>
/// <para>
/// This type is typically used alongside its public counterpart, <see cref="PublicKeys"/>,
/// to enable secure communication between entities.
/// </para>
/// </remarks>
public sealed class PrivateKeys
    : IEquatable<PrivateKeys>,
      IPrivateKeysProvider,
      ISigner,
      IDecrypter,
      IReferenceProvider,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    /// <summary>Gets the signing private key.</summary>
    public SigningPrivateKey SigningPrivateKey { get; }

    /// <summary>Gets the encapsulation private key.</summary>
    public EncapsulationPrivateKey EncapsulationPrivateKey { get; }

    /// <summary>
    /// Creates a new <see cref="PrivateKeys"/> from the given signing and encapsulation private keys.
    /// </summary>
    /// <param name="signingPrivateKey">The signing private key.</param>
    /// <param name="encapsulationPrivateKey">The encapsulation private key.</param>
    public PrivateKeys(SigningPrivateKey signingPrivateKey, EncapsulationPrivateKey encapsulationPrivateKey)
    {
        SigningPrivateKey = signingPrivateKey;
        EncapsulationPrivateKey = encapsulationPrivateKey;
    }

    /// <summary>
    /// Derives the corresponding <see cref="PublicKeys"/> from this private key set.
    /// </summary>
    /// <returns>The corresponding <see cref="PublicKeys"/>.</returns>
    public PublicKeys PublicKeys() =>
        new(
            SigningPrivateKey.PublicKey(),
            EncapsulationPrivateKey.PublicKey());

    // --- IPrivateKeysProvider ---

    /// <inheritdoc/>
    PrivateKeys IPrivateKeysProvider.PrivateKeys() => this;

    // --- ISigner ---

    /// <inheritdoc/>
    public Signature SignWithOptions(byte[] message, SigningOptions? options = null) =>
        SigningPrivateKey.SignWithOptions(message, options);

    // --- IDecrypter ---

    /// <inheritdoc/>
    EncapsulationPrivateKey IDecrypter.EncapsulationPrivateKey() => EncapsulationPrivateKey;

    /// <inheritdoc/>
    SymmetricKey IDecrypter.DecapsulateSharedSecret(EncapsulationCiphertext ciphertext) =>
        EncapsulationPrivateKey.DecapsulateSharedSecret(ciphertext);

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<PrivateKeys> ---

    /// <inheritdoc/>
    public bool Equals(PrivateKeys? other)
    {
        if (other is null) return false;
        return SigningPrivateKey == other.SigningPrivateKey
            && EncapsulationPrivateKey == other.EncapsulationPrivateKey;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PrivateKeys k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(SigningPrivateKey, EncapsulationPrivateKey);

    /// <summary>Tests equality of two PrivateKeys instances.</summary>
    public static bool operator ==(PrivateKeys? left, PrivateKeys? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two PrivateKeys instances.</summary>
    public static bool operator !=(PrivateKeys? left, PrivateKeys? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40013).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagPrivateKeys);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as a two-element array
    /// [signing_key_tagged_cbor, encapsulation_key_tagged_cbor].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            SigningPrivateKey.TaggedCbor(),
            EncapsulationPrivateKey.ToCbor(),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="PrivateKeys"/> from untagged CBOR (a two-element array).
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="PrivateKeys"/>.</returns>
    public static PrivateKeys FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("PrivateKeys",
                $"expected array of length 2, got {elements.Count}");

        var signingPrivateKey = SigningPrivateKey.FromTaggedCbor(elements[0]);
        var encapsulationPrivateKey = BCComponents.EncapsulationPrivateKey.FromCbor(elements[1]);
        return new PrivateKeys(signingPrivateKey, encapsulationPrivateKey);
    }

    /// <summary>Decodes a <see cref="PrivateKeys"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="PrivateKeys"/>.</returns>
    public static PrivateKeys FromTaggedCbor(Cbor cbor)
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

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() =>
        $"PrivateKeys({((IReferenceProvider)this).RefHexShort()}, {SigningPrivateKey}, {EncapsulationPrivateKey})";
}
