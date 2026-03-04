using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// An "Apparently Random Identifier" (ARID).
/// </summary>
/// <remarks>
/// <para>
/// An ARID is a cryptographically strong, universally unique identifier with
/// the following properties:
/// </para>
/// <list type="bullet">
/// <item>Non-correlatability: The bit sequence cannot be correlated with its
/// referent or any other ARID</item>
/// <item>Neutral semantics: Contains no inherent type information</item>
/// <item>Open generation: Any method of generation is allowed as long as it
/// produces statistically random bits</item>
/// <item>Minimum strength: Must be 256 bits (32 bytes) in length</item>
/// <item>Cryptographic suitability: Can be used as inputs to cryptographic
/// constructs</item>
/// </list>
/// <para>
/// Unlike digests/hashes which identify a fixed, immutable state of data,
/// ARIDs can serve as stable identifiers for mutable data structures.
/// </para>
/// </remarks>
public sealed class ARID
    : IEquatable<ARID>,
      IComparable<ARID>,
      IReferenceProvider,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    /// <summary>The size of an ARID in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private ARID(byte[] data)
    {
        _data = data;
    }

    /// <summary>Creates a new random ARID.</summary>
    /// <returns>A new <see cref="ARID"/>.</returns>
    public static ARID New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>Creates a new random ARID using the given RNG.</summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="ARID"/>.</returns>
    public static ARID NewUsing(IRandomNumberGenerator rng)
    {
        return new ARID(rng.RandomData(Size));
    }

    /// <summary>
    /// Creates an ARID from exactly <see cref="Size"/> bytes.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of ARID data.</param>
    /// <returns>A new <see cref="ARID"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 32 bytes.
    /// </exception>
    public static ARID FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("ARID", Size, data.Length);
        return new ARID((byte[])data.Clone());
    }

    /// <summary>Creates an ARID from a hexadecimal string.</summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="ARID"/>.</returns>
    public static ARID FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Returns a copy of the underlying 32-byte ARID data.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the ARID bytes as a copy.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Gets the ARID as a 64-character lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>Returns the first four bytes of the ARID as a hexadecimal string.</summary>
    public string ShortDescription() =>
        Convert.ToHexString(_data, 0, 4).ToLowerInvariant();

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IComparable<ARID> ---

    /// <inheritdoc/>
    public int CompareTo(ARID? other)
    {
        if (other is null) return 1;
        return _data.AsSpan().SequenceCompareTo(other._data);
    }

    // --- IEquatable<ARID> ---

    /// <inheritdoc/>
    public bool Equals(ARID? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ARID a && Equals(a);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two ARID instances.</summary>
    public static bool operator ==(ARID? left, ARID? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two ARID instances.</summary>
    public static bool operator !=(ARID? left, ARID? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40012).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagArid);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes an <see cref="ARID"/> from untagged CBOR (a byte string).</summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="ARID"/>.</returns>
    public static ARID FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>Decodes an <see cref="ARID"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="ARID"/>.</returns>
    public static ARID FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => $"ARID({Hex})";
}
