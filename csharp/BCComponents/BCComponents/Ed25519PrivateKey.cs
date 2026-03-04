using System.Security.Cryptography;
using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// An Ed25519 private key for creating digital signatures.
/// </summary>
/// <remarks>
/// <para>
/// Ed25519 is a public-key signature system based on the Edwards curve over the
/// finite field GF(2^255 - 19). It provides the following features:
/// </para>
/// <list type="bullet">
/// <item>Fast single-signature verification</item>
/// <item>Fast key generation</item>
/// <item>High security level (equivalent to 128 bits of symmetric security)</item>
/// <item>Collision resilience - hash function collisions don't break security</item>
/// <item>Protection against side-channel attacks</item>
/// <item>Small signatures (64 bytes) and small keys (32 bytes)</item>
/// </list>
/// <para>
/// Ed25519 private keys do not have direct CBOR serialization. They are wrapped
/// by <c>SigningPrivateKey</c> for CBOR encoding.
/// </para>
/// </remarks>
public sealed class Ed25519PrivateKey : IEquatable<Ed25519PrivateKey>, IReferenceProvider, IDisposable
{
    /// <summary>The size of an Ed25519 private key in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private Ed25519PrivateKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Generates a new random Ed25519 private key using the system's
    /// cryptographically secure RNG.
    /// </summary>
    /// <returns>A new random <see cref="Ed25519PrivateKey"/>.</returns>
    public static Ed25519PrivateKey New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates a new random Ed25519 private key using the given random number
    /// generator.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new random <see cref="Ed25519PrivateKey"/>.</returns>
    public static Ed25519PrivateKey NewUsing(IRandomNumberGenerator rng)
    {
        var data = rng.RandomData(Size);
        return new Ed25519PrivateKey(data);
    }

    /// <summary>
    /// Creates an Ed25519 private key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="Ed25519PrivateKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 32 bytes.
    /// </exception>
    public static Ed25519PrivateKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("Ed25519 private key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new Ed25519PrivateKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Creates an Ed25519 private key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="Ed25519PrivateKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the decoded data is not exactly 32 bytes.
    /// </exception>
    public static Ed25519PrivateKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>
    /// Derives a new Ed25519 private key from the given key material using HKDF.
    /// </summary>
    /// <param name="keyMaterial">The raw key material to derive from.</param>
    /// <returns>A deterministically derived <see cref="Ed25519PrivateKey"/>.</returns>
    public static Ed25519PrivateKey DeriveFromKeyMaterial(byte[] keyMaterial)
    {
        return new Ed25519PrivateKey(PublicKeyEncryption.DeriveSigningPrivateKey(keyMaterial));
    }

    /// <summary>
    /// Derives the <see cref="Ed25519PublicKey"/> corresponding to this private key.
    /// </summary>
    /// <returns>The public key derived from this private key.</returns>
    public Ed25519PublicKey PublicKey()
    {
        return Ed25519PublicKey.FromData(Ed25519Signing.Ed25519PublicKeyFromPrivateKey(_data));
    }

    /// <summary>
    /// Signs a message using Ed25519.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>A 64-byte Ed25519 signature.</returns>
    public byte[] Sign(byte[] message)
    {
        return Ed25519Signing.Ed25519Sign(_data, message);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(_data));

    // --- IEquatable<Ed25519PrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(Ed25519PrivateKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Ed25519PrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Ed25519PrivateKey instances.</summary>
    public static bool operator ==(Ed25519PrivateKey? left, Ed25519PrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Ed25519PrivateKey instances.</summary>
    public static bool operator !=(Ed25519PrivateKey? left, Ed25519PrivateKey? right) => !(left == right);

    // --- IDisposable ---

    /// <summary>Zeros the key material.</summary>
    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(_data);
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"Ed25519PrivateKey({((IReferenceProvider)this).RefHexShort()})";
}
