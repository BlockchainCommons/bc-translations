using System.Security.Cryptography;
using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A private key for X25519 key agreement operations.
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
/// Key features of X25519:
/// <list type="bullet">
/// <item>High security (128-bit security level)</item>
/// <item>High performance</item>
/// <item>Small key sizes (32 bytes)</item>
/// <item>Protection against various side-channel attacks</item>
/// </list>
/// </para>
/// </remarks>
public sealed class X25519PrivateKey : IEquatable<X25519PrivateKey>, ICborTaggedEncodable, ICborTaggedDecodable, IReferenceProvider, IDisposable
{
    /// <summary>The size of an X25519 private key in bytes.</summary>
    public const int KeySize = 32;

    private readonly byte[] _data;

    private X25519PrivateKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Generates a new random X25519 private key using the system's
    /// cryptographically secure RNG.
    /// </summary>
    /// <returns>A new random <see cref="X25519PrivateKey"/>.</returns>
    public static X25519PrivateKey New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates a new random X25519 private key using the given random number
    /// generator.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new random <see cref="X25519PrivateKey"/>.</returns>
    public static X25519PrivateKey NewUsing(IRandomNumberGenerator rng)
    {
        var data = PublicKeyEncryption.X25519NewPrivateKeyUsing(rng);
        return new X25519PrivateKey(data);
    }

    /// <summary>
    /// Generates a new random X25519 key pair using the system's
    /// cryptographically secure RNG.
    /// </summary>
    /// <returns>A tuple of (private key, public key).</returns>
    public static (X25519PrivateKey PrivateKey, X25519PublicKey PublicKey) Keypair()
    {
        var privateKey = New();
        return (privateKey, privateKey.PublicKey());
    }

    /// <summary>
    /// Generates a new random X25519 key pair using the given random number
    /// generator.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A tuple of (private key, public key).</returns>
    public static (X25519PrivateKey PrivateKey, X25519PublicKey PublicKey) KeypairUsing(IRandomNumberGenerator rng)
    {
        var privateKey = NewUsing(rng);
        return (privateKey, privateKey.PublicKey());
    }

    /// <summary>
    /// Creates an X25519 private key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="X25519PrivateKey"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="data"/> is not exactly 32 bytes.</exception>
    public static X25519PrivateKey FromData(byte[] data)
    {
        if (data.Length != KeySize)
            throw BCComponentsException.InvalidSize("X25519 private key", KeySize, data.Length);
        var copy = new byte[KeySize];
        Array.Copy(data, copy, KeySize);
        return new X25519PrivateKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Creates an X25519 private key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="X25519PrivateKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">Thrown if the decoded data is not exactly 32 bytes.</exception>
    public static X25519PrivateKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Derives the <see cref="X25519PublicKey"/> corresponding to this private key.
    /// </summary>
    /// <returns>The public key derived from this private key.</returns>
    public X25519PublicKey PublicKey()
    {
        return X25519PublicKey.FromData(PublicKeyEncryption.X25519PublicKeyFromPrivateKey(_data));
    }

    /// <summary>
    /// Derives an <see cref="X25519PrivateKey"/> from the given key material
    /// using HKDF.
    /// </summary>
    /// <param name="keyMaterial">The raw key material to derive from.</param>
    /// <returns>A deterministically derived <see cref="X25519PrivateKey"/>.</returns>
    public static X25519PrivateKey DeriveFromKeyMaterial(byte[] keyMaterial)
    {
        return new X25519PrivateKey(PublicKeyEncryption.DeriveAgreementPrivateKey(keyMaterial));
    }

    /// <summary>
    /// Derives a shared <see cref="SymmetricKey"/> from this private key and
    /// the given public key.
    /// </summary>
    /// <remarks>
    /// Both parties perform this operation with their own private key and
    /// the other party's public key, arriving at the same shared key.
    /// </remarks>
    /// <param name="publicKey">The other party's X25519 public key.</param>
    /// <returns>A shared <see cref="SymmetricKey"/> suitable for encryption.</returns>
    public SymmetricKey SharedKeyWith(X25519PublicKey publicKey)
    {
        return SymmetricKey.FromData(PublicKeyEncryption.X25519SharedKey(_data, publicKey.Data));
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the X25519PrivateKey type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagX25519PrivateKey);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="X25519PrivateKey"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="X25519PrivateKey"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    /// <exception cref="BCComponentsException">Thrown if the data is not exactly 32 bytes.</exception>
    public static X25519PrivateKey FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes an <see cref="X25519PrivateKey"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="X25519PrivateKey"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static X25519PrivateKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<X25519PrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(X25519PrivateKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is X25519PrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two X25519PrivateKey instances.</summary>
    public static bool operator ==(X25519PrivateKey? left, X25519PrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two X25519PrivateKey instances.</summary>
    public static bool operator !=(X25519PrivateKey? left, X25519PrivateKey? right) => !(left == right);

    // --- IDisposable ---

    /// <summary>Zeros the key material.</summary>
    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(_data);
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"X25519PrivateKey({((IReferenceProvider)this).RefHexShort()})";

    /// <summary>Returns a byte array copy of this key's data.</summary>
    /// <returns>A 32-byte array.</returns>
    public byte[] ToByteArray() => Data;
}
