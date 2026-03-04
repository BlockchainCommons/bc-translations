using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A private key for the ML-KEM post-quantum key encapsulation mechanism.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MLKEMPrivateKey"/> represents a private key that can be used to
/// decapsulate shared secrets using the ML-KEM (Module Lattice-based Key
/// Encapsulation Mechanism) post-quantum algorithm. It supports multiple
/// security levels:
/// </para>
/// <list type="bullet">
/// <item>ML-KEM-512: NIST security level 1 (roughly equivalent to AES-128), 1632 bytes</item>
/// <item>ML-KEM-768: NIST security level 3 (roughly equivalent to AES-192), 2400 bytes</item>
/// <item>ML-KEM-1024: NIST security level 5 (roughly equivalent to AES-256), 3168 bytes</item>
/// </list>
/// </remarks>
public sealed class MLKEMPrivateKey : IEquatable<MLKEMPrivateKey>, ICborTaggedEncodable, ICborTaggedDecodable, IDecrypter, IReferenceProvider
{
    private readonly MLKEMLevel _level;
    private readonly MLKemPrivateKeyParameters _params;
    private readonly byte[] _keyData;

    internal MLKEMPrivateKey(MLKEMLevel level, MLKemPrivateKeyParameters parameters, byte[] keyData)
    {
        _level = level;
        _params = parameters;
        _keyData = keyData;
    }

    /// <summary>
    /// Creates an <see cref="MLKEMPrivateKey"/> from BouncyCastle parameters.
    /// </summary>
    internal static MLKEMPrivateKey FromParameters(MLKEMLevel level, MLKemPrivateKeyParameters parameters)
    {
        var encoded = parameters.GetEncoded();
        return new MLKEMPrivateKey(level, parameters, encoded);
    }

    /// <summary>
    /// Creates an <see cref="MLKEMPrivateKey"/> from raw bytes and a security level.
    /// </summary>
    /// <param name="level">The security level of the key.</param>
    /// <param name="data">The raw bytes of the key.</param>
    /// <returns>A new <see cref="MLKEMPrivateKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the bytes do not represent a valid ML-KEM private key for the
    /// specified security level.
    /// </exception>
    public static MLKEMPrivateKey FromBytes(MLKEMLevel level, byte[] data)
    {
        try
        {
            var parameters = MLKemPrivateKeyParameters.FromEncoding(level.Parameters(), data);
            return new MLKEMPrivateKey(level, parameters, (byte[])data.Clone());
        }
        catch (Exception ex)
        {
            throw BCComponentsException.PostQuantum($"Invalid ML-KEM private key: {ex.Message}");
        }
    }

    /// <summary>Gets the security level of this ML-KEM private key.</summary>
    public MLKEMLevel Level => _level;

    /// <summary>Gets the size of this ML-KEM private key in bytes.</summary>
    public int Size => _level.PrivateKeySize();

    /// <summary>Returns a copy of the raw bytes of this ML-KEM private key.</summary>
    public byte[] AsBytes() => (byte[])_keyData.Clone();

    /// <summary>
    /// Decapsulates a shared secret from a ciphertext using this private key.
    /// </summary>
    /// <param name="ciphertext">The ciphertext containing the encapsulated shared secret.</param>
    /// <returns>A <see cref="SymmetricKey"/> containing the decapsulated shared secret.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the security level of the ciphertext does not match this key,
    /// or if decapsulation fails.
    /// </exception>
    public SymmetricKey DecapsulateSharedSecret(MLKEMCiphertext ciphertext)
    {
        if (ciphertext.Level != _level)
            throw BCComponentsException.PostQuantum("MLKEM level mismatch");

        try
        {
            var decapsulator = new MLKemDecapsulator(_level.Parameters());
            decapsulator.Init(_params);
            var ctBytes = ciphertext.AsBytes();
            var secBuf = new byte[decapsulator.SecretLength];
            decapsulator.Decapsulate(ctBytes, 0, ctBytes.Length, secBuf, 0, secBuf.Length);
            return SymmetricKey.FromData(secBuf);
        }
        catch (BCComponentsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw BCComponentsException.PostQuantum($"ML-KEM decapsulation failed: {ex.Message}");
        }
    }

    // --- IDecrypter ---

    /// <inheritdoc/>
    EncapsulationPrivateKey IDecrypter.EncapsulationPrivateKey() =>
        EncapsulationPrivateKey.FromMLKEM(this);

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the MLKEMPrivateKey type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagMlkemPrivateKey);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as a two-element array
    /// [level, key_bytes].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Cbor.FromInt((int)_level),
            Cbor.ToByteString(_keyData),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="MLKEMPrivateKey"/> from an untagged CBOR array.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be a two-element array).</param>
    /// <returns>A new <see cref="MLKEMPrivateKey"/>.</returns>
    public static MLKEMPrivateKey FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("MLKEMPrivateKey", $"must have two elements, got {elements.Count}");

        var levelValue = (int)elements[0].TryIntoUInt64();
        var level = MLKEMLevelExtensions.FromInt(levelValue);
        var data = elements[1].TryIntoByteString();
        return FromBytes(level, data);
    }

    /// <summary>
    /// Decodes an <see cref="MLKEMPrivateKey"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="MLKEMPrivateKey"/>.</returns>
    public static MLKEMPrivateKey FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<MLKEMPrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(MLKEMPrivateKey? other)
    {
        if (other is null) return false;
        return _level == other._level
            && _keyData.AsSpan().SequenceEqual(other._keyData);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MLKEMPrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_level);
        foreach (var b in _keyData)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two MLKEMPrivateKey instances.</summary>
    public static bool operator ==(MLKEMPrivateKey? left, MLKEMPrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two MLKEMPrivateKey instances.</summary>
    public static bool operator !=(MLKEMPrivateKey? left, MLKEMPrivateKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() =>
        $"{_level}PrivateKey({((IReferenceProvider)this).RefHexShort()})";
}
