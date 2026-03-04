using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// An uncompressed elliptic curve digital signature algorithm (ECDSA) public key.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="ECUncompressedPublicKey"/> is a 65-byte representation of a public
/// key on the secp256k1 curve. It consists of:
/// </para>
/// <list type="bullet">
/// <item>1 byte prefix (0x04)</item>
/// <item>32 bytes for the x-coordinate</item>
/// <item>32 bytes for the y-coordinate</item>
/// </list>
/// <para>
/// This format explicitly includes both coordinates of the elliptic curve point,
/// unlike the compressed format which only includes the x-coordinate and a single
/// byte to indicate the parity of the y-coordinate.
/// </para>
/// <para>
/// This is considered a legacy key type. The compressed format
/// (<see cref="ECPublicKey"/>) is more space-efficient and provides the same
/// cryptographic security. However, some legacy systems or protocols might require
/// the uncompressed format.
/// </para>
/// </remarks>
public sealed class ECUncompressedPublicKey
    : IEquatable<ECUncompressedPublicKey>,
      ICborTaggedEncodable,
      IReferenceProvider
{
    /// <summary>The size of an ECDSA uncompressed public key in bytes.</summary>
    public const int Size = 65;

    /// <summary>Legacy CBOR tag value (306) for backward compatibility.</summary>
    private const ulong TagEcKeyV1 = 306;

    private readonly byte[] _data;

    private ECUncompressedPublicKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates an EC uncompressed public key from a 65-byte array.
    /// </summary>
    /// <param name="data">Exactly 65 bytes of uncompressed public key data.</param>
    /// <returns>A new <see cref="ECUncompressedPublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 65 bytes.
    /// </exception>
    public static ECUncompressedPublicKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize(
                "ECDSA uncompressed public key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new ECUncompressedPublicKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 65-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Creates an EC uncompressed public key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 130-character hexadecimal string.</param>
    /// <returns>A new <see cref="ECUncompressedPublicKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the decoded data is not exactly 65 bytes.
    /// </exception>
    public static ECUncompressedPublicKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>
    /// Converts this uncompressed public key to its compressed form.
    /// </summary>
    /// <returns>The compressed public key.</returns>
    public ECPublicKey PublicKey()
    {
        return ECPublicKey.FromData(EcdsaKeys.EcdsaCompressPublicKey(_data));
    }

    /// <summary>
    /// Returns this uncompressed public key (returns self).
    /// </summary>
    /// <returns>This uncompressed public key.</returns>
    public ECUncompressedPublicKey UncompressedPublicKey() => this;

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the ECUncompressedPublicKey type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagEcKey, TagEcKeyV1);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The format is a map with:
    /// <list type="bullet">
    /// <item>Key 3: byte string of the key data</item>
    /// </list>
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromInt(3), Cbor.ToByteString(_data));
        return new Cbor(CborCase.Map(map));
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- IEquatable<ECUncompressedPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(ECUncompressedPublicKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ECUncompressedPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two ECUncompressedPublicKey instances.</summary>
    public static bool operator ==(ECUncompressedPublicKey? left, ECUncompressedPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two ECUncompressedPublicKey instances.</summary>
    public static bool operator !=(ECUncompressedPublicKey? left, ECUncompressedPublicKey? right) =>
        !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"ECUncompressedPublicKey({((IReferenceProvider)this).RefHexShort()})";
}
