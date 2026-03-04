using System.Security.Cryptography;
using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A symmetric encryption key used for both encryption and decryption.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SymmetricKey"/> is a 32-byte cryptographic key used with
/// ChaCha20-Poly1305 AEAD (Authenticated Encryption with Associated Data)
/// encryption. This implementation follows the IETF ChaCha20-Poly1305
/// specification as defined in
/// <see href="https://datatracker.ietf.org/doc/html/rfc8439">RFC 8439</see>.
/// </para>
/// <para>
/// Symmetric encryption uses the same key for both encryption and decryption,
/// unlike asymmetric encryption where different keys are used for each operation.
/// </para>
/// </remarks>
public sealed class SymmetricKey : IEquatable<SymmetricKey>, ICborTaggedEncodable, ICborTaggedDecodable, IReferenceProvider, IDisposable
{
    /// <summary>The size of a symmetric key in bytes.</summary>
    public const int Size = 32;

    private readonly byte[] _data;

    private SymmetricKey(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Generates a new random symmetric key using the system's cryptographically
    /// secure RNG.
    /// </summary>
    /// <returns>A new random <see cref="SymmetricKey"/>.</returns>
    public static SymmetricKey New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates a new random symmetric key using the given random number generator.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new random <see cref="SymmetricKey"/>.</returns>
    public static SymmetricKey NewUsing(IRandomNumberGenerator rng)
    {
        var data = rng.RandomData(Size);
        return new SymmetricKey(data);
    }

    /// <summary>
    /// Creates a symmetric key from a 32-byte array.
    /// </summary>
    /// <param name="data">Exactly 32 bytes of key data.</param>
    /// <returns>A new <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="data"/> is not exactly 32 bytes.</exception>
    public static SymmetricKey FromData(byte[] data)
    {
        if (data.Length != Size)
            throw BCComponentsException.InvalidSize("symmetric key", Size, data.Length);
        var copy = new byte[Size];
        Array.Copy(data, copy, Size);
        return new SymmetricKey(copy);
    }

    /// <summary>Gets a copy of the key data as a 32-byte array.</summary>
    public byte[] Data => (byte[])_data.Clone();

    /// <summary>Returns the key data as a read-only span.</summary>
    /// <returns>A read-only span over the key bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Creates a symmetric key from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A 64-character hexadecimal string.</param>
    /// <returns>A new <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    /// <exception cref="BCComponentsException">Thrown if the decoded data is not exactly 32 bytes.</exception>
    public static SymmetricKey FromHex(string hex)
    {
        var data = Convert.FromHexString(hex);
        return FromData(data);
    }

    /// <summary>Gets the key data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    // --- Encryption / Decryption ---

    /// <summary>
    /// Encrypts the given plaintext using this key with ChaCha20-Poly1305.
    /// </summary>
    /// <param name="plaintext">The data to encrypt.</param>
    /// <param name="aad">Optional additional authenticated data. If <c>null</c>, empty AAD is used.</param>
    /// <param name="nonce">Optional nonce. If <c>null</c>, a new random nonce is generated.</param>
    /// <returns>An <see cref="EncryptedMessage"/> containing the ciphertext, nonce, auth tag, and AAD.</returns>
    public EncryptedMessage Encrypt(byte[] plaintext, byte[]? aad = null, Nonce? nonce = null)
    {
        var effectiveAad = aad ?? Array.Empty<byte>();
        var effectiveNonce = nonce ?? Nonce.New();
        var (ciphertext, tag) = SymmetricEncryption.AeadChaCha20Poly1305EncryptWithAad(
            plaintext,
            _data,
            effectiveNonce.Data,
            effectiveAad);
        return new EncryptedMessage(ciphertext, effectiveAad, effectiveNonce, AuthenticationTag.FromData(tag));
    }

    /// <summary>
    /// Encrypts the given plaintext using this key and includes the specified
    /// digest in the AAD field as tagged CBOR.
    /// </summary>
    /// <param name="plaintext">The data to encrypt.</param>
    /// <param name="digest">The digest to include as AAD.</param>
    /// <param name="nonce">Optional nonce. If <c>null</c>, a new random nonce is generated.</param>
    /// <returns>An <see cref="EncryptedMessage"/> containing the ciphertext, nonce, auth tag, and digest AAD.</returns>
    public EncryptedMessage EncryptWithDigest(byte[] plaintext, Digest digest, Nonce? nonce = null)
    {
        var digestCborData = digest.TaggedCbor().ToCborData();
        return Encrypt(plaintext, digestCborData, nonce);
    }

    /// <summary>
    /// Decrypts the given <see cref="EncryptedMessage"/> using this key.
    /// </summary>
    /// <param name="message">The encrypted message to decrypt.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="BCComponentsException">Thrown if decryption fails.</exception>
    public byte[] Decrypt(EncryptedMessage message)
    {
        try
        {
            return SymmetricEncryption.AeadChaCha20Poly1305DecryptWithAad(
                message.Ciphertext,
                _data,
                message.Nonce.Data,
                message.Aad,
                message.AuthenticationTag.Data);
        }
        catch (Exception ex)
        {
            throw BCComponentsException.Crypto($"decryption failed: {ex.Message}");
        }
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the SymmetricKey type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagSymmetricKey);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="SymmetricKey"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    /// <exception cref="BCComponentsException">Thrown if the data is not exactly 32 bytes.</exception>
    public static SymmetricKey FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes a <see cref="SymmetricKey"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static SymmetricKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<SymmetricKey> ---

    /// <inheritdoc/>
    public bool Equals(SymmetricKey? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SymmetricKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two SymmetricKey instances.</summary>
    public static bool operator ==(SymmetricKey? left, SymmetricKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two SymmetricKey instances.</summary>
    public static bool operator !=(SymmetricKey? left, SymmetricKey? right) => !(left == right);

    // --- IDisposable ---

    /// <summary>Zeros the key material.</summary>
    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(_data);
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"SymmetricKey({((IReferenceProvider)this).RefHexShort()})";

    /// <summary>Returns a byte array copy of this key's data.</summary>
    /// <returns>A 32-byte array.</returns>
    public byte[] ToByteArray() => Data;
}
