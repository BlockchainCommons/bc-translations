using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCTags;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A compressed elliptic curve digital signature algorithm (ECDSA) public key.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="ECPublicKey"/> is a 33-byte compressed representation of a public
/// key on the secp256k1 curve. The first byte is a prefix (0x02 or 0x03) that
/// indicates the parity of the y-coordinate, followed by the 32-byte x-coordinate.
/// </para>
/// <para>
/// These public keys are used to:
/// </para>
/// <list type="bullet">
/// <item>Verify ECDSA signatures</item>
/// <item>Identify the owner of a private key without revealing the private key</item>
/// <item>Derive shared secrets (when combined with another party's private key)</item>
/// </list>
/// <para>
/// Unlike the larger 65-byte uncompressed format (<see cref="ECUncompressedPublicKey"/>),
/// compressed public keys save space while providing the same cryptographic security.
/// </para>
/// </remarks>
public sealed class ECPublicKey
    : IEquatable<ECPublicKey>,
      ICborTagged,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      IUREncodable,
      IReferenceProvider
{
    /// <summary>The size of an ECDSA compressed public key in bytes.</summary>
    public const int Size = 33;

    /// <summary>Legacy CBOR tag value (306) for backward compatibility.</summary>
    private const ulong TagEcKeyV1 = 306;

    private readonly byte[] _data;

    private ECPublicKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates an EC public key from a 33-byte array.
    /// </summary>
    /// <param name="data">Exactly 33 bytes of compressed public key data.</param>
    /// <returns>A new <see cref="ECPublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 33 bytes.
    /// </exception>
    public static ECPublicKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("ECDSA public key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new ECPublicKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 33-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Creates an EC public key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 66-character hexadecimal string.</param>
    /// <returns>A new <see cref="ECPublicKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the decoded data is not exactly 33 bytes.
    /// </exception>
    public static ECPublicKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>
    /// Returns the compressed public key (returns self).
    /// </summary>
    /// <returns>This public key.</returns>
    public ECPublicKey PublicKey() => this;

    /// <summary>
    /// Converts this compressed public key to its uncompressed form.
    /// </summary>
    /// <returns>The uncompressed public key.</returns>
    public ECUncompressedPublicKey UncompressedPublicKey()
    {
        return ECUncompressedPublicKey.FromData(EcdsaKeys.EcdsaDecompressPublicKey(_data));
    }

    /// <summary>
    /// Verifies an ECDSA signature for a message using this public key.
    /// </summary>
    /// <param name="signature">A 64-byte ECDSA signature.</param>
    /// <param name="message">The message that was signed.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid for the given message and this
    /// public key; <c>false</c> otherwise.
    /// </returns>
    public bool Verify(byte[] signature, byte[] message)
    {
        return EcdsaSigning.EcdsaVerify(_data, signature, message);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the ECPublicKey type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagEcKey, TagEcKeyV1);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The format is a map with:
    /// <list type="bullet">
    /// <item>Key 3: byte string of the key data (note the absence of key 2,
    /// which would indicate a private key)</item>
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

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="ECPublicKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (a map).</param>
    /// <returns>A new <see cref="ECPublicKey"/>.</returns>
    /// <exception cref="CborException">
    /// Thrown if the CBOR is not a valid map or contains unexpected types.
    /// </exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if a private key flag is present or the data is invalid.
    /// </exception>
    public static ECPublicKey FromUntaggedCbor(Cbor cbor)
    {
        var map = cbor.TryIntoMap();

        // If key 2 is present, this is a private key, not a public key
        var isPrivateCbor = map.GetValue(Cbor.FromInt(2));
        if (isPrivateCbor is not null)
            throw BCComponentsException.InvalidData("EC key", "expected public key but found private key flag (key 2)");

        var dataCbor = map.Extract(Cbor.FromInt(3));
        var data = dataCbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes an <see cref="ECPublicKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="ECPublicKey"/>.</returns>
    /// <exception cref="CborException">
    /// Thrown if the CBOR tag does not match or the data is invalid.
    /// </exception>
    public static ECPublicKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<ECPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(ECPublicKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ECPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two ECPublicKey instances.</summary>
    public static bool operator ==(ECPublicKey? left, ECPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two ECPublicKey instances.</summary>
    public static bool operator !=(ECPublicKey? left, ECPublicKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"ECPublicKey({((IReferenceProvider)this).RefHexShort()})";
}
