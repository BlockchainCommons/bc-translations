using System.Security.Cryptography;
using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A private key for elliptic curve digital signature algorithms on secp256k1.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="ECPrivateKey"/> is a 32-byte secret value that can be used to:
/// </para>
/// <list type="bullet">
/// <item>Generate its corresponding public key</item>
/// <item>Sign messages using the ECDSA signature scheme</item>
/// <item>Sign messages using the Schnorr signature scheme (BIP-340)</item>
/// </list>
/// <para>
/// These keys use the secp256k1 curve, which is the same curve used in Bitcoin
/// and other cryptocurrencies. The secp256k1 curve is defined by the Standards
/// for Efficient Cryptography Group (SECG).
/// </para>
/// </remarks>
public sealed class ECPrivateKey
    : IEquatable<ECPrivateKey>,
      ICborTagged,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      IUREncodable,
      IReferenceProvider,
      IDisposable
{
    /// <summary>The size of an ECDSA private key in bytes.</summary>
    public const int Size = 32;

    /// <summary>Legacy CBOR tag value (306) for backward compatibility.</summary>
    private const ulong TagEcKeyV1 = 306;

    private readonly byte[] _data;

    private ECPrivateKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Generates a new random EC private key using the system's
    /// cryptographically secure RNG.
    /// </summary>
    /// <returns>A new random <see cref="ECPrivateKey"/>.</returns>
    public static ECPrivateKey New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates a new random EC private key using the given random number
    /// generator.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new random <see cref="ECPrivateKey"/>.</returns>
    public static ECPrivateKey NewUsing(IRandomNumberGenerator rng)
    {
        var data = rng.RandomData(Size);
        return new ECPrivateKey(data);
    }

    /// <summary>
    /// Creates an EC private key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="ECPrivateKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 32 bytes.
    /// </exception>
    public static ECPrivateKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("EC private key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new ECPrivateKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Creates an EC private key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="ECPrivateKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the decoded data is not exactly 32 bytes.
    /// </exception>
    public static ECPrivateKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>
    /// Derives a new EC private key from the given key material using HKDF.
    /// </summary>
    /// <param name="keyMaterial">The raw key material to derive from.</param>
    /// <returns>A deterministically derived <see cref="ECPrivateKey"/>.</returns>
    public static ECPrivateKey DeriveFromKeyMaterial(byte[] keyMaterial)
    {
        return new ECPrivateKey(PublicKeyEncryption.DeriveSigningPrivateKey(keyMaterial));
    }

    /// <summary>
    /// Derives the corresponding ECDSA compressed public key.
    /// </summary>
    /// <returns>The compressed public key derived from this private key.</returns>
    public ECPublicKey PublicKey()
    {
        return ECPublicKey.FromData(EcdsaKeys.EcdsaPublicKeyFromPrivateKey(_data));
    }

    /// <summary>
    /// Derives the Schnorr (x-only) public key from this private key.
    /// </summary>
    /// <remarks>
    /// Schnorr public keys are used with the BIP-340 Schnorr signature scheme.
    /// Unlike ECDSA public keys, Schnorr public keys are 32 bytes ("x-only")
    /// rather than 33 bytes.
    /// </remarks>
    /// <returns>The Schnorr public key derived from this private key.</returns>
    public SchnorrPublicKey SchnorrPublicKey()
    {
        return BCComponents.SchnorrPublicKey.FromData(EcdsaKeys.SchnorrPublicKeyFromPrivateKey(_data));
    }

    /// <summary>
    /// Signs a message using the ECDSA signature scheme.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>A 64-byte ECDSA signature.</returns>
    public byte[] EcdsaSign(byte[] message)
    {
        return EcdsaSigning.EcdsaSign(_data, message);
    }

    /// <summary>
    /// Signs a message using the Schnorr signature scheme.
    /// </summary>
    /// <remarks>
    /// Uses the secure random number generator for nonce generation.
    /// </remarks>
    /// <param name="message">The message to sign.</param>
    /// <returns>A 64-byte Schnorr signature.</returns>
    public byte[] SchnorrSign(byte[] message)
    {
        return SchnorrSigning.SchnorrSign(_data, message);
    }

    /// <summary>
    /// Signs a message using the Schnorr signature scheme with a custom random
    /// number generator.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <param name="rng">The random number generator to use for nonce generation.</param>
    /// <returns>A 64-byte Schnorr signature.</returns>
    public byte[] SchnorrSignUsing(byte[] message, IRandomNumberGenerator rng)
    {
        return SchnorrSigning.SchnorrSignUsing(_data, message, rng);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the ECPrivateKey type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagEcKey, TagEcKeyV1);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The format is a map with:
    /// <list type="bullet">
    /// <item>Key 2: boolean <c>true</c> (indicates private key)</item>
    /// <item>Key 3: byte string of the key data</item>
    /// </list>
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromInt(2), Cbor.True());
        map.Insert(Cbor.FromInt(3), Cbor.ToByteString(_data));
        return new Cbor(CborCase.Map(map));
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="ECPrivateKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (a map).</param>
    /// <returns>A new <see cref="ECPrivateKey"/>.</returns>
    /// <exception cref="CborException">
    /// Thrown if the CBOR is not a valid map or contains unexpected types.
    /// </exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the private key flag is missing or the data is invalid.
    /// </exception>
    public static ECPrivateKey FromUntaggedCbor(Cbor cbor)
    {
        var map = cbor.TryIntoMap();

        var isPrivateCbor = map.GetValue(Cbor.FromInt(2));
        if (isPrivateCbor is null)
            throw BCComponentsException.InvalidData("EC key", "missing private key flag (key 2)");
        var isPrivate = isPrivateCbor.TryIntoBool();
        if (!isPrivate)
            throw BCComponentsException.InvalidData("EC key", "expected private key (key 2 = true)");

        var dataCbor = map.Extract(Cbor.FromInt(3));
        var data = dataCbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes an <see cref="ECPrivateKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="ECPrivateKey"/>.</returns>
    /// <exception cref="CborException">
    /// Thrown if the CBOR tag does not match or the data is invalid.
    /// </exception>
    public static ECPrivateKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<ECPrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(ECPrivateKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ECPrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two ECPrivateKey instances.</summary>
    public static bool operator ==(ECPrivateKey? left, ECPrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two ECPrivateKey instances.</summary>
    public static bool operator !=(ECPrivateKey? left, ECPrivateKey? right) => !(left == right);

    // --- IDisposable ---

    /// <summary>Zeros the key material.</summary>
    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(_data);
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"ECPrivateKey({((IReferenceProvider)this).RefHexShort()})";
}
