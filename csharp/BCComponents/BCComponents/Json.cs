using System.Text;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A CBOR-tagged container for UTF-8 JSON text.
/// </summary>
/// <remarks>
/// The <see cref="Json"/> type wraps UTF-8 JSON text as a CBOR byte string
/// with tag 262. This allows JSON data to be embedded within CBOR structures
/// while maintaining type information through the tag.
///
/// This implementation does not validate that the contained data is well-formed
/// JSON. It simply provides a type-safe wrapper around byte data that is
/// intended to contain JSON text.
///
/// Named "Json" (not "JSON") to follow C# naming conventions.
/// </remarks>
public sealed class Json : IEquatable<Json>, ICborTaggedEncodable, ICborTaggedDecodable
{
    private readonly byte[] _data;

    private Json(byte[] data)
    {
        _data = data;
    }

    /// <summary>Gets the length of the JSON data in bytes.</summary>
    public int Length => _data.Length;

    /// <summary>Gets whether the JSON data is empty.</summary>
    public bool IsEmpty => _data.Length == 0;

    /// <summary>
    /// Creates a new <see cref="Json"/> instance from byte data.
    /// </summary>
    /// <param name="data">The UTF-8 encoded JSON bytes.</param>
    /// <returns>A new <see cref="Json"/>.</returns>
    public static Json FromData(byte[] data)
    {
        var copy = new byte[data.Length];
        Array.Copy(data, copy, data.Length);
        return new Json(copy);
    }

    /// <summary>
    /// Creates a new <see cref="Json"/> instance from a string.
    /// </summary>
    /// <param name="s">The JSON string.</param>
    /// <returns>A new <see cref="Json"/>.</returns>
    public static Json FromString(string s)
    {
        return new Json(Encoding.UTF8.GetBytes(s));
    }

    /// <summary>Returns the JSON data as a read-only span of bytes.</summary>
    /// <returns>A read-only span over the JSON bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Returns the JSON data as a UTF-8 string.
    /// </summary>
    /// <returns>The JSON string.</returns>
    /// <exception cref="DecoderFallbackException">Thrown if the data is not valid UTF-8.</exception>
    public string AsString() => Encoding.UTF8.GetString(_data);

    /// <summary>
    /// Creates a new <see cref="Json"/> instance from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A hexadecimal string representing UTF-8 JSON bytes.</param>
    /// <returns>A new <see cref="Json"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    public static Json FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Gets the JSON data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the Json type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagJson);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="Json"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="Json"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    public static Json FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes a <see cref="Json"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="Json"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static Json FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<Json> ---

    /// <inheritdoc/>
    public bool Equals(Json? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Json j && Equals(j);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Json instances.</summary>
    public static bool operator ==(Json? left, Json? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Json instances.</summary>
    public static bool operator !=(Json? left, Json? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"JSON({AsString()})";

    /// <summary>Returns a byte array copy of this JSON's data.</summary>
    /// <returns>A byte array.</returns>
    public byte[] ToByteArray() => (byte[])_data.Clone();
}
