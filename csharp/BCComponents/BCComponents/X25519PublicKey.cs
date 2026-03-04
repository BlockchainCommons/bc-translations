using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A public key for X25519 key agreement operations.
/// </summary>
/// <remarks>
/// <para>
/// X25519 is an elliptic-curve Diffie-Hellman key exchange protocol based on
/// Curve25519 as defined in
/// <see href="https://datatracker.ietf.org/doc/html/rfc7748">RFC 7748</see>.
/// It allows two parties to establish a shared secret key over an insecure
/// channel.
/// </para>
/// <para>
/// The X25519 public key is generated from a corresponding private key and is
/// designed to be:
/// <list type="bullet">
/// <item>Compact (32 bytes)</item>
/// <item>Fast to use in key agreement operations</item>
/// <item>Resistant to various cryptographic attacks</item>
/// </list>
/// </para>
/// </remarks>
public sealed class X25519PublicKey : IEquatable<X25519PublicKey>, ICborTaggedEncodable, ICborTaggedDecodable, IReferenceProvider
{
    /// <summary>The size of an X25519 public key in bytes.</summary>
    public const int KeySize = 32;

    private readonly byte[] _data;

    private X25519PublicKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates an X25519 public key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="X25519PublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="data"/> is not exactly 32 bytes.</exception>
    public static X25519PublicKey FromData(byte[] data)
    {
        if (data.Length != KeySize)
            throw BCComponentsException.InvalidSize("X25519 public key", KeySize, data.Length);
        var copy = new byte[KeySize];
        Array.Copy(data, copy, KeySize);
        return new X25519PublicKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Creates an X25519 public key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="X25519PublicKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">Thrown if the decoded data is not exactly 32 bytes.</exception>
    public static X25519PublicKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the X25519PublicKey type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagX25519PublicKey);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="X25519PublicKey"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="X25519PublicKey"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    /// <exception cref="BCComponentsException">Thrown if the data is not exactly 32 bytes.</exception>
    public static X25519PublicKey FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes an <see cref="X25519PublicKey"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="X25519PublicKey"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static X25519PublicKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<X25519PublicKey> ---

    /// <inheritdoc/>
    public bool Equals(X25519PublicKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is X25519PublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two X25519PublicKey instances.</summary>
    public static bool operator ==(X25519PublicKey? left, X25519PublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two X25519PublicKey instances.</summary>
    public static bool operator !=(X25519PublicKey? left, X25519PublicKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"X25519PublicKey({((IReferenceProvider)this).RefHexShort()})";

    /// <summary>Returns a byte array copy of this key's data.</summary>
    /// <returns>A 32-byte array.</returns>
    public byte[] ToByteArray() => Data;
}
