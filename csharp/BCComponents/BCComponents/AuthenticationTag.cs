using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// The authentication tag produced by the encryption process to verify message
/// integrity.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="AuthenticationTag"/> is a 16-byte value generated during
/// ChaCha20-Poly1305 authenticated encryption. It serves as a message
/// authentication code (MAC) that verifies both the authenticity and integrity
/// of the encrypted message.
/// </para>
/// <para>
/// During decryption, the tag is verified to ensure:
/// <list type="bullet">
/// <item>The message has not been tampered with (integrity)</item>
/// <item>The message was encrypted by someone who possesses the encryption key (authenticity)</item>
/// </list>
/// </para>
/// <para>
/// This implementation follows the Poly1305 MAC algorithm as specified in
/// <see href="https://datatracker.ietf.org/doc/html/rfc8439">RFC 8439</see>.
/// </para>
/// </remarks>
public sealed class AuthenticationTag : IEquatable<AuthenticationTag>
{
    /// <summary>The size of an authentication tag in bytes.</summary>
    public const int Size = 16;

    private readonly byte[] _data;

    private AuthenticationTag(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a new authentication tag from a 16-byte array.
    /// </summary>
    /// <param name="data">Exactly 16 bytes of tag data.</param>
    /// <returns>A new <see cref="AuthenticationTag"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="data"/> is not exactly 16 bytes.</exception>
    public static AuthenticationTag FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("authentication tag", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new AuthenticationTag(copy);
    }

    /// <summary>Gets a copy of the tag data as a 16-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the tag data as a read-only span.</summary>
    /// <returns>A read-only span over the tag bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>Gets the tag data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    // --- CBOR (untagged, used as part of EncryptedMessage) ---

    /// <summary>
    /// Encodes this authentication tag as a CBOR byte string (not tagged).
    /// </summary>
    /// <returns>A CBOR byte string containing the tag data.</returns>
    public Cbor ToCbor() => Cbor.ToByteString(_data);

    /// <summary>
    /// Decodes an <see cref="AuthenticationTag"/> from a CBOR byte string.
    /// </summary>
    /// <param name="cbor">The CBOR byte string to decode.</param>
    /// <returns>A new <see cref="AuthenticationTag"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    /// <exception cref="BCComponentsException">Thrown if the data is not exactly 16 bytes.</exception>
    public static AuthenticationTag FromCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    // --- IEquatable<AuthenticationTag> ---

    /// <inheritdoc/>
    public bool Equals(AuthenticationTag? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AuthenticationTag t && Equals(t);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two AuthenticationTag instances.</summary>
    public static bool operator ==(AuthenticationTag? left, AuthenticationTag? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two AuthenticationTag instances.</summary>
    public static bool operator !=(AuthenticationTag? left, AuthenticationTag? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"AuthenticationTag({Hex})";

    /// <summary>Returns a byte array copy of this tag's data.</summary>
    /// <returns>A 16-byte array.</returns>
    public byte[] ToByteArray() => Data;
}
