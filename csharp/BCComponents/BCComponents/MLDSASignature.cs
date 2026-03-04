using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A digital signature created with the ML-DSA post-quantum signature algorithm.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MLDSASignature"/> represents a digital signature created using the
/// ML-DSA (Module Lattice-based Digital Signature Algorithm) post-quantum algorithm.
/// It supports multiple security levels:
/// </para>
/// <list type="bullet">
/// <item><see cref="MLDSALevel.MLDSA44"/>: NIST security level 2</item>
/// <item><see cref="MLDSALevel.MLDSA65"/>: NIST security level 3</item>
/// <item><see cref="MLDSALevel.MLDSA87"/>: NIST security level 5</item>
/// </list>
/// </remarks>
public sealed class MLDSASignature : IEquatable<MLDSASignature>, ICborTaggedEncodable, ICborTaggedDecodable
{
    private readonly byte[] _data;

    /// <summary>The security level of this signature.</summary>
    public MLDSALevel Level { get; }

    private MLDSASignature(MLDSALevel level, byte[] data)
    {
        Level = level;
        _data = data;
    }

    /// <summary>
    /// Creates an ML-DSA signature from raw bytes and a security level.
    /// </summary>
    /// <param name="level">The security level of the signature.</param>
    /// <param name="data">The raw signature bytes.</param>
    /// <returns>A new <see cref="MLDSASignature"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the data length does not match the expected signature size for the level.
    /// </exception>
    public static MLDSASignature FromBytes(MLDSALevel level, byte[] data)
    {
        var expectedSize = level.SignatureSize();
        if (data.Length != expectedSize)
            throw BCComponentsException.InvalidSize($"ML-DSA {level} signature", expectedSize, data.Length);
        var copy = new byte[data.Length];
        Array.Copy(data, copy, data.Length);
        return new MLDSASignature(level, copy);
    }

    /// <summary>Returns the raw signature bytes.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Returns the raw signature bytes as a read-only span.</summary>
    internal ReadOnlySpan<byte> AsBytesSpan() => _data;

    /// <summary>Returns the size of this signature in bytes.</summary>
    public int Size => _data.Length;

    // --- IEquatable<MLDSASignature> ---

    /// <inheritdoc/>
    public bool Equals(MLDSASignature? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MLDSASignature s && Equals(s);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Level);
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two MLDSASignature instances.</summary>
    public static bool operator ==(MLDSASignature? left, MLDSASignature? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two MLDSASignature instances.</summary>
    public static bool operator !=(MLDSASignature? left, MLDSASignature? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagMldsaSignature);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as an array [level, byte_string].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Level.ToCbor(),
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
    /// Decodes an <see cref="MLDSASignature"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be an array of [level, byte_string]).</param>
    /// <returns>A new <see cref="MLDSASignature"/>.</returns>
    public static MLDSASignature FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("MLDSASignature", "must have exactly 2 elements");
        var level = MLDSALevelExtensions.MLDSALevelFromCbor(elements[0]);
        var data = elements[1].TryIntoByteString();
        return FromBytes(level, data);
    }

    /// <summary>
    /// Decodes an <see cref="MLDSASignature"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="MLDSASignature"/>.</returns>
    public static MLDSASignature FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => $"MLDSASignature({Level})";
}
