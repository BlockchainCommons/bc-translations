using BlockchainCommons.BCTags;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// An eXtensible IDentifier (XID).
/// </summary>
/// <remarks>
/// <para>
/// A XID is a unique 32-byte identifier for a subject entity (person,
/// organization, device, or any other entity). XIDs have the following
/// characteristics:
/// </para>
/// <list type="bullet">
/// <item>They are cryptographically tied to a public key at inception (the
/// "inception key")</item>
/// <item>They remain stable throughout their lifecycle even as their keys and
/// permissions change</item>
/// <item>They can be extended to XID documents containing keys, endpoints,
/// permissions, and delegation info</item>
/// <item>They support key rotation and multiple verification schemes</item>
/// </list>
/// <para>
/// A XID is created by taking the SHA-256 hash of the CBOR encoding of a
/// public signing key. This ensures the XID is cryptographically tied to the key.
/// </para>
/// </remarks>
public sealed class XID
    : IEquatable<XID>,
      IComparable<XID>,
      IReferenceProvider,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    /// <summary>The size of a XID in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private XID(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a new XID from the given signing public key (the "genesis key").
    /// </summary>
    /// <remarks>
    /// The XID is the SHA-256 digest of the tagged CBOR encoding of the public key.
    /// </remarks>
    /// <param name="key">The signing public key.</param>
    /// <returns>A new <see cref="XID"/>.</returns>
    public static XID FromSigningPublicKey(SigningPublicKey key)
    {
        var keyCborData = key.TaggedCbor().ToCborData();
        var digest = Digest.FromImage(keyCborData);
        return new XID(digest.Data);
    }

    /// <summary>
    /// Creates a XID from exactly <see cref="Size"/> bytes.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of XID data.</param>
    /// <returns>A new <see cref="XID"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 32 bytes.
    /// </exception>
    public static XID FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("XID", Size, data.Length);
        return new XID((byte[])data.Clone());
    }

    /// <summary>Creates a XID from a hexadecimal string.</summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="XID"/>.</returns>
    public static XID FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Returns a copy of the underlying 32-byte XID data.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the XID bytes as a copy.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Gets the XID as a 64-character lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>Returns the first four bytes of the XID as a hexadecimal string.</summary>
    public string ShortDescription() => ((IReferenceProvider)this).RefHexShort();

    /// <summary>
    /// Validates the XID against the given public signing key.
    /// </summary>
    /// <param name="key">The signing public key to validate against.</param>
    /// <returns>
    /// <c>true</c> if this XID matches the SHA-256 hash of the key's CBOR encoding.
    /// </returns>
    public bool Validate(SigningPublicKey key)
    {
        var keyCborData = key.TaggedCbor().ToCborData();
        var digest = Digest.FromImage(keyCborData);
        return _data.AsSpan().SequenceEqual(digest.Data);
    }

    /// <summary>
    /// Returns the first four bytes of the XID as upper-case ByteWords.
    /// </summary>
    /// <param name="prefix">If <c>true</c>, the XID marker prefix is prepended.</param>
    /// <returns>The ByteWords identifier string.</returns>
    public string BytewordsIdentifier(bool prefix = false)
    {
        var shortData = ((IReferenceProvider)this).RefDataShort();
        var s = Bytewords.Identifier(shortData).ToUpperInvariant();
        return prefix ? $"\uD83C\uDD67 {s}" : s;
    }

    /// <summary>
    /// Returns the first four bytes of the XID as Bytemoji.
    /// </summary>
    /// <param name="prefix">If <c>true</c>, the XID marker prefix is prepended.</param>
    /// <returns>The Bytemoji identifier string.</returns>
    public string BytemojiIdentifier(bool prefix = false)
    {
        var shortData = ((IReferenceProvider)this).RefDataShort();
        var s = Bytewords.BytemojiIdentifier(shortData).ToUpperInvariant();
        return prefix ? $"\uD83C\uDD67 {s}" : s;
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    /// <remarks>
    /// Unlike most types, a XID's reference is directly from its data, not from
    /// the digest of its tagged CBOR.
    /// </remarks>
    public Reference Reference() => BCComponents.Reference.FromData(_data);

    // --- IComparable<XID> ---

    /// <inheritdoc/>
    public int CompareTo(XID? other)
    {
        if (other is null) return 1;
        return _data.AsSpan().SequenceCompareTo(other._data);
    }

    // --- IEquatable<XID> ---

    /// <inheritdoc/>
    public bool Equals(XID? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is XID x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two XID instances.</summary>
    public static bool operator ==(XID? left, XID? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two XID instances.</summary>
    public static bool operator !=(XID? left, XID? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40024).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagXid);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes a <see cref="XID"/> from untagged CBOR (a byte string).</summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="XID"/>.</returns>
    public static XID FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>Decodes a <see cref="XID"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="XID"/>.</returns>
    public static XID FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => $"XID({ShortDescription()})";
}

/// <summary>
/// A type that can provide a XID.
/// </summary>
public interface IXIDProvider
{
    /// <summary>Returns the XID for this object.</summary>
    XID Xid();
}
