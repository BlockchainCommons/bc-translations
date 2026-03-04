using BlockchainCommons.BCTags;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Implementors of this interface provide a globally unique reference
/// to themselves.
/// </summary>
public interface IReferenceProvider
{
    /// <summary>
    /// Returns a cryptographic reference that uniquely identifies this object.
    /// </summary>
    Reference Reference();

    /// <summary>Returns the reference data as a hexadecimal string.</summary>
    string RefHex() => Reference().RefHex;

    /// <summary>Returns the first four bytes of the reference.</summary>
    byte[] RefDataShort() => Reference().RefDataShort;

    /// <summary>Returns the first four bytes of the reference as a hexadecimal string.</summary>
    string RefHexShort() => Reference().RefHexShort;

    /// <summary>
    /// Returns the first four bytes of the reference as upper-case ByteWords.
    /// </summary>
    /// <param name="prefix">An optional prefix to add before the ByteWords representation.</param>
    string RefBytewords(string? prefix = null) => Reference().BytewordsIdentifier(prefix);

    /// <summary>
    /// Returns the first four bytes of the reference as Bytemoji.
    /// </summary>
    /// <param name="prefix">An optional prefix to add before the Bytemoji representation.</param>
    string RefBytemoji(string? prefix = null) => Reference().BytemojiIdentifier(prefix);
}

/// <summary>
/// A globally unique reference to a globally unique object.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Reference"/> provides a cryptographically secure way to
/// uniquely identify objects based on their content. It is a fixed-size
/// (32 bytes) identifier, typically derived from the SHA-256 hash of the
/// object's serialized form.
/// </para>
/// <para>
/// A <see cref="Reference"/> can be displayed in various formats including
/// hexadecimal, ByteWords, and Bytemoji.
/// </para>
/// </remarks>
public sealed class Reference
    : IEquatable<Reference>,
      IComparable<Reference>,
      ICborTaggedEncodable,
      IDigestProvider,
      IReferenceProvider
{
    /// <summary>The size of a reference in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private Reference(byte[] data)
    {
        _data = data;
    }

    /// <summary>Creates a new reference from exactly 32 bytes.</summary>
    /// <param name="data">Exactly 32 bytes of reference data.</param>
    /// <returns>A new <see cref="Reference"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="data"/> is not exactly <see cref="Size"/> bytes.
    /// </exception>
    public static Reference FromData(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length != Size)
        {
            throw BCComponentsException.InvalidSize("reference", Size, data.Length);
        }
        return new Reference((byte[])data.Clone());
    }

    /// <summary>Creates a new reference from the given digest.</summary>
    /// <param name="digest">The digest to derive the reference from.</param>
    /// <returns>A new <see cref="Reference"/> instance.</returns>
    public static Reference FromDigest(Digest digest)
    {
        return FromData(digest.Data);
    }

    /// <summary>Gets a copy of the reference data.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Gets a copy of the reference data as a byte array.</summary>
    public byte[] AsBytes() => Data;

    /// <summary>
    /// Creates a reference from a 64-character hexadecimal string.
    /// </summary>
    /// <param name="hex">The hexadecimal string.</param>
    /// <returns>A new <see cref="Reference"/> instance.</returns>
    public static Reference FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Gets the full reference data as a 64-character hexadecimal string.</summary>
    public string RefHex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>Gets the first four bytes of the reference.</summary>
    public byte[] RefDataShort => _data[..4];

    /// <summary>Gets the first four bytes of the reference as an 8-character hexadecimal string.</summary>
    public string RefHexShort => Convert.ToHexString(_data, 0, 4).ToLowerInvariant();

    /// <summary>
    /// Returns the first four bytes of the reference as upper-case ByteWords.
    /// </summary>
    /// <param name="prefix">An optional prefix to prepend.</param>
    /// <returns>The ByteWords identifier string.</returns>
    public string BytewordsIdentifier(string? prefix = null)
    {
        var shortData = RefDataShort;
        var s = Bytewords.Identifier(shortData).ToUpperInvariant();
        return prefix is not null ? $"{prefix} {s}" : s;
    }

    /// <summary>
    /// Returns the first four bytes of the reference as Bytemoji.
    /// </summary>
    /// <param name="prefix">An optional prefix to prepend.</param>
    /// <returns>The Bytemoji identifier string.</returns>
    public string BytemojiIdentifier(string? prefix = null)
    {
        var shortData = RefDataShort;
        var s = Bytewords.BytemojiIdentifier(shortData).ToUpperInvariant();
        return prefix is not null ? $"{prefix} {s}" : s;
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    Reference IReferenceProvider.Reference() => Reference.FromDigest(GetDigest());

    // --- IDigestProvider ---

    /// <inheritdoc/>
    public Digest GetDigest()
    {
        return Digest.FromImage(TaggedCbor().ToCborData());
    }

    // --- CBOR ---

    /// <summary>Gets the CBOR tags for this type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagReference);

    /// <inheritdoc/>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <inheritdoc/>
    public Cbor ToCbor() => TaggedCbor();

    /// <inheritdoc/>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <inheritdoc/>
    public byte[] TaggedCborData() => TaggedCbor().ToCborData();

    /// <summary>Decodes a <see cref="Reference"/> from untagged CBOR.</summary>
    /// <param name="cbor">The CBOR value to decode.</param>
    /// <returns>The decoded <see cref="Reference"/> instance.</returns>
    public static Reference FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>Decodes a <see cref="Reference"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value to decode.</param>
    /// <returns>The decoded <see cref="Reference"/> instance.</returns>
    public static Reference FromTaggedCbor(Cbor cbor)
    {
        var tags = CborTags;
        foreach (var tag in tags)
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

    // --- Equality ---

    /// <inheritdoc/>
    public bool Equals(Reference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Reference);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data) hash.Add(b);
        return hash.ToHashCode();
    }

    public static bool operator ==(Reference? a, Reference? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(Reference? a, Reference? b) => !(a == b);

    // --- IComparable ---

    /// <inheritdoc/>
    public int CompareTo(Reference? other)
    {
        if (other is null) return 1;
        return _data.AsSpan().SequenceCompareTo(other._data);
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"Reference({RefHexShort})";

    /// <summary>Gets a debug representation showing the full hex.</summary>
    public string DebugDescription => $"Reference({RefHex})";
}
