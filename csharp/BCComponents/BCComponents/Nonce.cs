using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A random nonce ("number used once").
/// </summary>
/// <remarks>
/// A <see cref="Nonce"/> is a cryptographic primitive consisting of a random
/// or pseudo-random number that is used only once in a cryptographic
/// communication. Nonces are often used in authentication protocols, encryption
/// algorithms, and digital signatures to prevent replay attacks and ensure
/// the uniqueness of encrypted messages.
///
/// In this implementation, a nonce is a 12-byte random value. The size is
/// chosen to be sufficiently large to prevent collisions while remaining
/// efficient for storage and transmission.
/// </remarks>
public sealed class Nonce : IEquatable<Nonce>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>The size of a nonce in bytes.</summary>
    public const int Size = 12;

    private readonly byte[] _data;

    private Nonce(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Generates a new random nonce using the system's cryptographically secure RNG.
    /// </summary>
    /// <returns>A new random <see cref="Nonce"/>.</returns>
    public static Nonce New()
    {
        var data = new byte[Size];
        SecureRandomNumberGenerator.SecureFillRandomData(data);
        return new Nonce(data);
    }

    /// <summary>
    /// Creates a nonce from a 12-byte array.
    /// </summary>
    /// <param name="data">Exactly 12 bytes of nonce data.</param>
    /// <returns>A new <see cref="Nonce"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="data"/> is not exactly 12 bytes.</exception>
    public static Nonce FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("nonce", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new Nonce(copy);
    }

    /// <summary>Gets a copy of the nonce data as a 12-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the nonce data as a read-only span.</summary>
    /// <returns>A read-only span over the nonce bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Creates a nonce from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 24-character hexadecimal string.</param>
    /// <returns>A new <see cref="Nonce"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">Thrown if the decoded data is not exactly 12 bytes.</exception>
    public static Nonce FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>Gets the nonce data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the Nonce type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagNonce);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="Nonce"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="Nonce"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    /// <exception cref="BCComponentsException">Thrown if the data is not exactly 12 bytes.</exception>
    public static Nonce FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes a <see cref="Nonce"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="Nonce"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static Nonce FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<Nonce> ---

    /// <inheritdoc/>
    public bool Equals(Nonce? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Nonce n && Equals(n);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Nonce instances.</summary>
    public static bool operator ==(Nonce? left, Nonce? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Nonce instances.</summary>
    public static bool operator !=(Nonce? left, Nonce? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"Nonce({Hex})";

    /// <summary>Returns a byte array copy of this nonce's data.</summary>
    /// <returns>A 12-byte array.</returns>
    public byte[] ToByteArray() => Data;
}
