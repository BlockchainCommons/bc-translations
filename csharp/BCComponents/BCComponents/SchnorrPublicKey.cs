using BlockchainCommons.BCCrypto;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A Schnorr (x-only) elliptic curve public key.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="SchnorrPublicKey"/> is a 32-byte "x-only" public key used with
/// the BIP-340 Schnorr signature scheme. Unlike compressed ECDSA public keys
/// (33 bytes) that include a prefix byte indicating the parity of the
/// y-coordinate, Schnorr public keys only contain the x-coordinate of the
/// elliptic curve point.
/// </para>
/// <para>
/// Schnorr signatures offer several advantages over traditional ECDSA signatures:
/// </para>
/// <list type="bullet">
/// <item><b>Linearity</b>: Enables key and signature aggregation (e.g., for
/// multisignature schemes)</item>
/// <item><b>Non-malleability</b>: Prevents third parties from modifying signatures</item>
/// <item><b>Smaller size</b>: Signatures are 64 bytes vs 70-72 bytes for ECDSA</item>
/// <item><b>Better privacy</b>: Makes different multisig policies indistinguishable</item>
/// <item><b>Provable security</b>: Requires fewer cryptographic assumptions than ECDSA</item>
/// </list>
/// <para>
/// Schnorr public keys do not have direct CBOR tag support and are not directly
/// serialized via UR.
/// </para>
/// </remarks>
public sealed class SchnorrPublicKey : IEquatable<SchnorrPublicKey>, IReferenceProvider
{
    /// <summary>The size of a Schnorr public key in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private SchnorrPublicKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a Schnorr public key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="SchnorrPublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is not exactly 32 bytes.
    /// </exception>
    public static SchnorrPublicKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("Schnorr public key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new SchnorrPublicKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Creates a Schnorr public key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="SchnorrPublicKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">
    /// Thrown if the decoded data is not exactly 32 bytes.
    /// </exception>
    public static SchnorrPublicKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>
    /// Verifies a Schnorr signature for a message using this public key.
    /// </summary>
    /// <remarks>
    /// This implementation follows the BIP-340 Schnorr signature verification
    /// algorithm.
    /// </remarks>
    /// <param name="signature">A 64-byte Schnorr signature.</param>
    /// <param name="message">The message that was signed.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid for the given message and this
    /// public key; <c>false</c> otherwise.
    /// </returns>
    public bool SchnorrVerify(byte[] signature, byte[] message)
    {
        return SchnorrSigning.SchnorrVerify(_data, signature, message);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(_data));

    // --- IEquatable<SchnorrPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(SchnorrPublicKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SchnorrPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two SchnorrPublicKey instances.</summary>
    public static bool operator ==(SchnorrPublicKey? left, SchnorrPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two SchnorrPublicKey instances.</summary>
    public static bool operator !=(SchnorrPublicKey? left, SchnorrPublicKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"SchnorrPublicKey({((IReferenceProvider)this).RefHexShort()})";
}
