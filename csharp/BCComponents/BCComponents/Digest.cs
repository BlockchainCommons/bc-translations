using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCTags;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A cryptographically secure digest, implemented with SHA-256.
/// </summary>
/// <remarks>
/// A <see cref="Digest"/> represents the cryptographic hash of some data.
/// SHA-256 is used, producing a 32-byte hash value. Digests are used
/// throughout the library for data verification and as unique identifiers
/// derived from data.
/// </remarks>
public sealed class Digest : IEquatable<Digest>, IComparable<Digest>, ICborTagged, ICborTaggedEncodable, ICborTaggedDecodable, IUREncodable, IURDecodable, IDigestProvider
{
    /// <summary>The size of a digest in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private Digest(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a new digest from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of digest data.</param>
    /// <returns>A new <see cref="Digest"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is not exactly 32 bytes.</exception>
    public static Digest FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("digest", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new Digest(copy);
    }


    /// <summary>
    /// Creates a new digest by hashing the given image with SHA-256.
    /// </summary>
    /// <param name="image">The data to hash.</param>
    /// <returns>A new <see cref="Digest"/> containing the SHA-256 hash.</returns>
    public static Digest FromImage(byte[] image)
    {
        return new Digest(Hash.Sha256(image));
    }

    /// <summary>
    /// Creates a new digest by concatenating image parts and hashing with SHA-256.
    /// </summary>
    /// <param name="imageParts">The data parts to concatenate and hash.</param>
    /// <returns>A new <see cref="Digest"/>.</returns>
    public static Digest FromImageParts(byte[][] imageParts)
    {
        int totalLen = 0;
        foreach (var part in imageParts)
            totalLen += part.Length;

        var buf = new byte[totalLen];
        int offset = 0;
        foreach (var part in imageParts)
        {
            Array.Copy(part, 0, buf, offset, part.Length);
            offset += part.Length;
        }
        return FromImage(buf);
    }

    /// <summary>
    /// Creates a new digest from an array of digests by concatenating their data and hashing.
    /// </summary>
    /// <param name="digests">The digests to combine.</param>
    /// <returns>A new <see cref="Digest"/> computed from the concatenated digest data.</returns>
    public static Digest FromDigests(Digest[] digests)
    {
        var buf = new byte[digests.Length * Size];
        for (int i = 0; i < digests.Length; i++)
        {
            Array.Copy(digests[i]._data, 0, buf, i * Size, Size);
        }
        return FromImage(buf);
    }

    /// <summary>Gets a copy of the digest data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the digest data as a read-only span.</summary>
    /// <returns>A read-only span over the digest bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Validates that this digest matches the SHA-256 hash of the given image.
    /// </summary>
    /// <param name="image">The data to validate against.</param>
    /// <returns><c>true</c> if the digest matches the hash of <paramref name="image"/>.</returns>
    public bool Validate(byte[] image)
    {
        return Equals(FromImage(image));
    }

    /// <summary>
    /// Creates a new digest from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="Digest"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">Thrown if the decoded data is not exactly 32 bytes.</exception>
    public static Digest FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>Gets the digest data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>Returns a short description of the digest (first 4 bytes as hex).</summary>
    /// <returns>An 8-character hexadecimal string.</returns>
    public string ShortDescription() => Convert.ToHexString(_data, 0, 4).ToLowerInvariant();

    /// <summary>
    /// Validates the given data against an optional digest.
    /// </summary>
    /// <param name="image">The data to validate.</param>
    /// <param name="digest">The digest to validate against, or <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="digest"/> is <c>null</c> or if it matches
    /// the SHA-256 hash of <paramref name="image"/>.
    /// </returns>
    public static bool ValidateOpt(byte[] image, Digest? digest)
    {
        return digest == null || digest.Validate(image);
    }

    // --- IDigestProvider ---

    /// <summary>Returns itself, as a Digest is already a digest.</summary>
    /// <returns>This digest.</returns>
    Digest IDigestProvider.GetDigest() => this;

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the Digest type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagDigest);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation (used by IUREncodable).</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="Digest"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="Digest"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    /// <exception cref="BCComponentsException">Thrown if the data is not exactly 32 bytes.</exception>
    public static Digest FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes a <see cref="Digest"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="Digest"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static Digest FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<Digest> ---

    /// <inheritdoc/>
    public bool Equals(Digest? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Digest d && Equals(d);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Digest instances.</summary>
    public static bool operator ==(Digest? left, Digest? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Digest instances.</summary>
    public static bool operator !=(Digest? left, Digest? right) => !(left == right);

    // --- IComparable<Digest> ---

    /// <summary>
    /// Compares this digest to another lexicographically by their underlying bytes.
    /// </summary>
    /// <param name="other">The other digest to compare to.</param>
    /// <returns>A negative, zero, or positive value indicating relative order.</returns>
    public int CompareTo(Digest? other)
    {
        if (other is null) return 1;
        return _data.AsSpan().SequenceCompareTo(other._data);
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"Digest({Hex})";

    /// <summary>Returns a byte array copy of this digest's data.</summary>
    /// <returns>A 32-byte array.</returns>
    public byte[] ToByteArray() => Data;
}
