using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A public key for the ML-KEM post-quantum key encapsulation mechanism.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MLKEMPublicKey"/> represents a public key that can be used to
/// encapsulate shared secrets using the ML-KEM (Module Lattice-based Key
/// Encapsulation Mechanism) post-quantum algorithm. It supports multiple
/// security levels:
/// </para>
/// <list type="bullet">
/// <item>ML-KEM-512: NIST security level 1 (roughly equivalent to AES-128), 800 bytes</item>
/// <item>ML-KEM-768: NIST security level 3 (roughly equivalent to AES-192), 1184 bytes</item>
/// <item>ML-KEM-1024: NIST security level 5 (roughly equivalent to AES-256), 1568 bytes</item>
/// </list>
/// </remarks>
public sealed class MLKEMPublicKey : IEquatable<MLKEMPublicKey>, ICborTaggedEncodable, ICborTaggedDecodable, IReferenceProvider
{
    private readonly MLKEMLevel _level;
    private readonly MLKemPublicKeyParameters _params;
    private readonly byte[] _keyData;

    internal MLKEMPublicKey(MLKEMLevel level, MLKemPublicKeyParameters parameters, byte[] keyData)
    {
        _level = level;
        _params = parameters;
        _keyData = keyData;
    }

    /// <summary>
    /// Creates an <see cref="MLKEMPublicKey"/> from BouncyCastle parameters.
    /// </summary>
    internal static MLKEMPublicKey FromParameters(MLKEMLevel level, MLKemPublicKeyParameters parameters)
    {
        var encoded = parameters.GetEncoded();
        return new MLKEMPublicKey(level, parameters, encoded);
    }

    /// <summary>
    /// Creates an <see cref="MLKEMPublicKey"/> from raw bytes and a security level.
    /// </summary>
    /// <param name="level">The security level of the key.</param>
    /// <param name="data">The raw bytes of the key.</param>
    /// <returns>A new <see cref="MLKEMPublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the bytes do not represent a valid ML-KEM public key for the
    /// specified security level.
    /// </exception>
    public static MLKEMPublicKey FromBytes(MLKEMLevel level, byte[] data)
    {
        try
        {
            var parameters = MLKemPublicKeyParameters.FromEncoding(level.Parameters(), data);
            return new MLKEMPublicKey(level, parameters, (byte[])data.Clone());
        }
        catch (Exception ex)
        {
            throw BCComponentsException.PostQuantum($"Invalid ML-KEM public key: {ex.Message}");
        }
    }

    /// <summary>Gets the security level of this ML-KEM public key.</summary>
    public MLKEMLevel Level => _level;

    /// <summary>Gets the size of this ML-KEM public key in bytes.</summary>
    public int Size => _level.PublicKeySize();

    /// <summary>Returns a copy of the raw bytes of this ML-KEM public key.</summary>
    public byte[] AsBytes() => (byte[])_keyData.Clone();

    /// <summary>
    /// Encapsulates a new shared secret using this public key.
    /// </summary>
    /// <returns>
    /// A tuple containing the shared secret as a <see cref="SymmetricKey"/>
    /// and the <see cref="MLKEMCiphertext"/>.
    /// </returns>
    public (SymmetricKey SharedKey, MLKEMCiphertext Ciphertext) EncapsulateNewSharedSecret()
    {
        var encapsulator = new MLKemEncapsulator(_level.Parameters());
        encapsulator.Init(_params);
        var encBuf = new byte[encapsulator.EncapsulationLength];
        var secBuf = new byte[encapsulator.SecretLength];
        encapsulator.Encapsulate(encBuf, 0, encBuf.Length, secBuf, 0, secBuf.Length);

        var sharedKey = SymmetricKey.FromData(secBuf);
        var ciphertext = MLKEMCiphertext.FromBytes(_level, encBuf);

        return (sharedKey, ciphertext);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the MLKEMPublicKey type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagMlkemPublicKey);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as a two-element array
    /// [level, key_bytes].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Cbor.FromInt((int)_level),
            Cbor.ToByteString(_keyData),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="MLKEMPublicKey"/> from an untagged CBOR array.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be a two-element array).</param>
    /// <returns>A new <see cref="MLKEMPublicKey"/>.</returns>
    public static MLKEMPublicKey FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("MLKEMPublicKey", $"must have two elements, got {elements.Count}");

        var levelValue = (int)elements[0].TryIntoUInt64();
        var level = MLKEMLevelExtensions.FromInt(levelValue);
        var data = elements[1].TryIntoByteString();
        return FromBytes(level, data);
    }

    /// <summary>
    /// Decodes an <see cref="MLKEMPublicKey"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="MLKEMPublicKey"/>.</returns>
    public static MLKEMPublicKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<MLKEMPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(MLKEMPublicKey? other)
    {
        if (other is null) return false;
        return _level == other._level
            && _keyData.AsSpan().SequenceEqual(other._keyData);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MLKEMPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_level);
        foreach (var b in _keyData)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two MLKEMPublicKey instances.</summary>
    public static bool operator ==(MLKEMPublicKey? left, MLKEMPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two MLKEMPublicKey instances.</summary>
    public static bool operator !=(MLKEMPublicKey? left, MLKEMPublicKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() =>
        $"{_level}PublicKey({((IReferenceProvider)this).RefHexShort()})";
}
