using BlockchainCommons.BCCrypto;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// An Ed25519 public key for verifying digital signatures.
/// </summary>
/// <remarks>
/// <para>
/// Ed25519 public keys are used to verify signatures created with the
/// corresponding private key. The Ed25519 signature system provides:
/// </para>
/// <list type="bullet">
/// <item>Fast signature verification</item>
/// <item>Small public keys (32 bytes)</item>
/// <item>High security with resistance to various attacks</item>
/// </list>
/// <para>
/// Ed25519 public keys do not have direct CBOR serialization. They are wrapped
/// by <c>SigningPublicKey</c> for CBOR encoding.
/// </para>
/// </remarks>
public sealed class Ed25519PublicKey : IEquatable<Ed25519PublicKey>, IReferenceProvider
{
    /// <summary>The size of an Ed25519 public key in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private Ed25519PublicKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates an Ed25519 public key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="Ed25519PublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 32 bytes.
    /// </exception>
    public static Ed25519PublicKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("Ed25519 public key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new Ed25519PublicKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Creates an Ed25519 public key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="Ed25519PublicKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the decoded data is not exactly 32 bytes.
    /// </exception>
    public static Ed25519PublicKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>
    /// Verifies an Ed25519 signature for a message using this public key.
    /// </summary>
    /// <param name="signature">A 64-byte Ed25519 signature.</param>
    /// <param name="message">The message that was signed.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid for the given message and this
    /// public key; <c>false</c> otherwise.
    /// </returns>
    public bool Verify(byte[] signature, byte[] message)
    {
        return Ed25519Signing.Ed25519Verify(_data, message, signature);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(_data));

    // --- IEquatable<Ed25519PublicKey> ---

    /// <inheritdoc/>
    public bool Equals(Ed25519PublicKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Ed25519PublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Ed25519PublicKey instances.</summary>
    public static bool operator ==(Ed25519PublicKey? left, Ed25519PublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Ed25519PublicKey instances.</summary>
    public static bool operator !=(Ed25519PublicKey? left, Ed25519PublicKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"Ed25519PublicKey({((IReferenceProvider)this).RefHexShort()})";
}
