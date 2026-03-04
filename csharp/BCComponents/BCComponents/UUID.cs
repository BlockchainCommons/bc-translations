using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A Universally Unique Identifier (UUID).
/// </summary>
/// <remarks>
/// <para>
/// UUIDs are 128-bit (16-byte) identifiers designed to be unique across space
/// and time. This implementation creates type 4 (random) UUIDs following the
/// UUID specification:
/// </para>
/// <list type="bullet">
/// <item>Version field (bits 48-51) is set to 4, indicating a random UUID</item>
/// <item>Variant field (bits 64-65) is set to 2, indicating RFC 4122 variant</item>
/// </list>
/// <para>
/// The canonical textual representation uses 5 groups separated by hyphens:
/// <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>
/// </para>
/// <para>
/// Note: This is the bc-components UUID type, not <see cref="System.Guid"/>.
/// </para>
/// </remarks>
public sealed class UUID : IEquatable<UUID>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>The size of a UUID in bytes.</summary>
    public const int Size = 16;

    private readonly byte[] _data;

    private UUID(byte[] data)
    {
        _data = data;
    }

    /// <summary>Creates a new random type 4 UUID.</summary>
    /// <returns>A new <see cref="UUID"/>.</returns>
    public static UUID New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>Creates a new random type 4 UUID using the given RNG.</summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="UUID"/>.</returns>
    public static UUID NewUsing(IRandomNumberGenerator rng)
    {
        var bytes = rng.RandomData(Size);
        // Set version to 4 (random UUID)
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40);
        // Set variant to 2 (RFC 4122)
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new UUID(bytes);
    }

    /// <summary>
    /// Creates a UUID from exactly <see cref="Size"/> bytes.
    /// </summary>
    /// <param name="data">Exactly 16 bytes of UUID data.</param>
    /// <returns>A new <see cref="UUID"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 16 bytes.
    /// </exception>
    public static UUID FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("UUID", Size, data.Length);
        return new UUID((byte[])data.Clone());
    }

    /// <summary>
    /// Parses a UUID from the canonical string representation.
    /// </summary>
    /// <remarks>
    /// Accepts the standard format: <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>
    /// </remarks>
    /// <param name="uuidString">The UUID string to parse.</param>
    /// <returns>A new <see cref="UUID"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the string is not a valid UUID.
    /// </exception>
    public static UUID FromString(string uuidString)
    {
        var stripped = uuidString.Trim().Replace("-", "");
        var bytes = Convert.FromHexString(stripped);
        return FromData(bytes);
    }

    /// <summary>Creates a UUID from a hexadecimal string (no dashes).</summary>
    /// <param name="hex">A 32-character hexadecimal string.</param>
    /// <returns>A new <see cref="UUID"/>.</returns>
    public static UUID FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Returns a copy of the underlying 16-byte UUID data.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the UUID bytes as a copy.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    // --- IEquatable<UUID> ---

    /// <inheritdoc/>
    public bool Equals(UUID? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is UUID u && Equals(u);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two UUID instances.</summary>
    public static bool operator ==(UUID? left, UUID? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two UUID instances.</summary>
    public static bool operator !=(UUID? left, UUID? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (37).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagUuid);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes a <see cref="UUID"/> from untagged CBOR (a byte string).</summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="UUID"/>.</returns>
    public static UUID FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>Decodes a <see cref="UUID"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="UUID"/>.</returns>
    public static UUID FromTaggedCbor(Cbor cbor)
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

    /// <summary>
    /// Returns the canonical UUID string representation.
    /// </summary>
    /// <remarks>
    /// Format: <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c>
    /// </remarks>
    public override string ToString()
    {
        var hex = Convert.ToHexString(_data).ToLowerInvariant();
        return $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..32]}";
    }
}
