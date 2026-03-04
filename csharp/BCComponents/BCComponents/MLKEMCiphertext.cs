using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A ciphertext containing an encapsulated shared secret for ML-KEM.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MLKEMCiphertext"/> represents a ciphertext produced by the ML-KEM
/// (Module Lattice-based Key Encapsulation Mechanism) post-quantum algorithm
/// during the encapsulation process. It contains an encapsulated shared secret
/// that can only be recovered by the corresponding private key.
/// </para>
/// <para>
/// It supports multiple security levels:
/// </para>
/// <list type="bullet">
/// <item>ML-KEM-512: NIST security level 1 (roughly equivalent to AES-128), 768 bytes</item>
/// <item>ML-KEM-768: NIST security level 3 (roughly equivalent to AES-192), 1088 bytes</item>
/// <item>ML-KEM-1024: NIST security level 5 (roughly equivalent to AES-256), 1568 bytes</item>
/// </list>
/// </remarks>
public sealed class MLKEMCiphertext : IEquatable<MLKEMCiphertext>, ICborTaggedEncodable, ICborTaggedDecodable
{
    private readonly MLKEMLevel _level;
    private readonly byte[] _data;

    private MLKEMCiphertext(MLKEMLevel level, byte[] data)
    {
        _level = level;
        _data = data;
    }

    /// <summary>
    /// Creates an <see cref="MLKEMCiphertext"/> from raw bytes and a security level.
    /// </summary>
    /// <param name="level">The security level of the ciphertext.</param>
    /// <param name="data">The raw bytes of the ciphertext.</param>
    /// <returns>A new <see cref="MLKEMCiphertext"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the bytes do not represent a valid ML-KEM ciphertext for the
    /// specified security level.
    /// </exception>
    public static MLKEMCiphertext FromBytes(MLKEMLevel level, byte[] data)
    {
        var expectedSize = level.CiphertextSize();
        if (data.Length != expectedSize)
            throw BCComponentsException.InvalidSize($"ML-KEM ciphertext ({level})", expectedSize, data.Length);
        return new MLKEMCiphertext(level, (byte[])data.Clone());
    }

    /// <summary>Gets the security level of this ML-KEM ciphertext.</summary>
    public MLKEMLevel Level => _level;

    /// <summary>Gets the size of this ML-KEM ciphertext in bytes.</summary>
    public int Size => _level.CiphertextSize();

    /// <summary>Returns a copy of the raw bytes of this ML-KEM ciphertext.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the MLKEMCiphertext type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagMlkemCiphertext);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as a two-element array
    /// [level, ciphertext_bytes].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Cbor.FromInt((int)_level),
            Cbor.ToByteString(_data),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="MLKEMCiphertext"/> from an untagged CBOR array.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be a two-element array).</param>
    /// <returns>A new <see cref="MLKEMCiphertext"/>.</returns>
    public static MLKEMCiphertext FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("MLKEMCiphertext", $"must have two elements, got {elements.Count}");

        var levelValue = (int)elements[0].TryIntoUInt64();
        var level = MLKEMLevelExtensions.FromInt(levelValue);
        var data = elements[1].TryIntoByteString();
        return FromBytes(level, data);
    }

    /// <summary>
    /// Decodes an <see cref="MLKEMCiphertext"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="MLKEMCiphertext"/>.</returns>
    public static MLKEMCiphertext FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<MLKEMCiphertext> ---

    /// <inheritdoc/>
    public bool Equals(MLKEMCiphertext? other)
    {
        if (other is null) return false;
        return _level == other._level
            && _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MLKEMCiphertext ct && Equals(ct);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_level);
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two MLKEMCiphertext instances.</summary>
    public static bool operator ==(MLKEMCiphertext? left, MLKEMCiphertext? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two MLKEMCiphertext instances.</summary>
    public static bool operator !=(MLKEMCiphertext? left, MLKEMCiphertext? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"{_level}Ciphertext";
}
