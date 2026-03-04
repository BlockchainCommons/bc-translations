using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A container combining signing and encapsulation public keys.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PublicKeys"/> packages a <see cref="SigningPublicKey"/> for verifying
/// digital signatures with an <see cref="EncapsulationPublicKey"/> for encrypting
/// messages, providing a complete public key set for secure communication with
/// an entity.
/// </para>
/// <para>
/// This type is designed to be freely shared across networks and systems,
/// allowing others to securely communicate with the key owner, who holds the
/// corresponding <see cref="PrivateKeys"/> instance.
/// </para>
/// </remarks>
public sealed class PublicKeys
    : IEquatable<PublicKeys>,
      IPublicKeysProvider,
      IVerifier,
      IEncrypter,
      IReferenceProvider,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    /// <summary>Gets the signing public key.</summary>
    public SigningPublicKey SigningPublicKey { get; }

    /// <summary>Gets the encapsulation public key.</summary>
    public EncapsulationPublicKey EncapsulationPublicKey { get; }

    /// <summary>
    /// Creates a new <see cref="PublicKeys"/> from the given signing and encapsulation public keys.
    /// </summary>
    /// <param name="signingPublicKey">The signing public key.</param>
    /// <param name="encapsulationPublicKey">The encapsulation public key.</param>
    public PublicKeys(SigningPublicKey signingPublicKey, EncapsulationPublicKey encapsulationPublicKey)
    {
        SigningPublicKey = signingPublicKey;
        EncapsulationPublicKey = encapsulationPublicKey;
    }

    // --- IPublicKeysProvider ---

    /// <inheritdoc/>
    PublicKeys IPublicKeysProvider.PublicKeys() => this;

    // --- IVerifier ---

    /// <inheritdoc/>
    public bool Verify(Signature signature, byte[] message) =>
        SigningPublicKey.Verify(signature, message);

    // --- IEncrypter ---

    /// <inheritdoc/>
    EncapsulationPublicKey IEncrypter.EncapsulationPublicKey() => EncapsulationPublicKey;

    /// <inheritdoc/>
    (SymmetricKey SharedKey, EncapsulationCiphertext Ciphertext) IEncrypter.EncapsulateNewSharedSecret() =>
        EncapsulationPublicKey.EncapsulateNewSharedSecret();

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<PublicKeys> ---

    /// <inheritdoc/>
    public bool Equals(PublicKeys? other)
    {
        if (other is null) return false;
        return SigningPublicKey == other.SigningPublicKey
            && EncapsulationPublicKey == other.EncapsulationPublicKey;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PublicKeys k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(SigningPublicKey, EncapsulationPublicKey);

    /// <summary>Tests equality of two PublicKeys instances.</summary>
    public static bool operator ==(PublicKeys? left, PublicKeys? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two PublicKeys instances.</summary>
    public static bool operator !=(PublicKeys? left, PublicKeys? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40017).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagPublicKeys);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as a two-element array
    /// [signing_key_tagged_cbor, encapsulation_key_tagged_cbor].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            SigningPublicKey.TaggedCbor(),
            EncapsulationPublicKey.ToCbor(),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="PublicKeys"/> from untagged CBOR (a two-element array).
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="PublicKeys"/>.</returns>
    public static PublicKeys FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("PublicKeys",
                $"expected array of length 2, got {elements.Count}");

        var signingPublicKey = SigningPublicKey.FromTaggedCbor(elements[0]);
        var encapsulationPublicKey = BCComponents.EncapsulationPublicKey.FromCbor(elements[1]);
        return new PublicKeys(signingPublicKey, encapsulationPublicKey);
    }

    /// <summary>Decodes a <see cref="PublicKeys"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="PublicKeys"/>.</returns>
    public static PublicKeys FromTaggedCbor(Cbor cbor)
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
        $"PublicKeys({((IReferenceProvider)this).RefHexShort()}, {SigningPublicKey}, {EncapsulationPublicKey})";
}
