using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A public key for the ML-DSA post-quantum digital signature algorithm.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MLDSAPublicKey"/> represents a public key that can verify digital
/// signatures created with the ML-DSA (Module Lattice-based Digital Signature
/// Algorithm) post-quantum algorithm. It supports multiple security levels:
/// </para>
/// <list type="bullet">
/// <item><see cref="MLDSALevel.MLDSA44"/>: NIST security level 2</item>
/// <item><see cref="MLDSALevel.MLDSA65"/>: NIST security level 3</item>
/// <item><see cref="MLDSALevel.MLDSA87"/>: NIST security level 5</item>
/// </list>
/// </remarks>
public sealed class MLDSAPublicKey
    : IEquatable<MLDSAPublicKey>,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      IReferenceProvider
{
    private readonly byte[] _data;

    /// <summary>The security level of this public key.</summary>
    public MLDSALevel Level { get; }

    private MLDSAPublicKey(MLDSALevel level, byte[] data)
    {
        Level = level;
        _data = data;
    }

    /// <summary>
    /// Creates an ML-DSA public key from raw bytes and a security level.
    /// </summary>
    /// <param name="level">The security level of the key.</param>
    /// <param name="data">The raw key bytes.</param>
    /// <returns>A new <see cref="MLDSAPublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the data length does not match the expected key size for the level.
    /// </exception>
    public static MLDSAPublicKey FromBytes(MLDSALevel level, byte[] data)
    {
        var expectedSize = level.PublicKeySize();
        if (data.Length != expectedSize)
            throw BCComponentsException.InvalidSize($"ML-DSA {level} public key", expectedSize, data.Length);
        var copy = new byte[data.Length];
        Array.Copy(data, copy, data.Length);
        return new MLDSAPublicKey(level, copy);
    }

    /// <summary>Returns the raw key bytes.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Returns the raw key bytes as a read-only span.</summary>
    internal ReadOnlySpan<byte> AsBytesSpan() => _data;

    /// <summary>Returns the size of this key in bytes.</summary>
    public int Size => _data.Length;

    /// <summary>
    /// Verifies an ML-DSA signature for a message using this public key.
    /// </summary>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="message">The message that was signed.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid for the message and this public key;
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the security level of the signature does not match this key's level.
    /// </exception>
    public bool Verify(MLDSASignature signature, byte[] message)
    {
        if (signature.Level != Level)
            throw BCComponentsException.LevelMismatch();

        try
        {
            var parameters = MLDSAPrivateKey.GetParameters(Level);
            var keyParams = MLDsaPublicKeyParameters.FromEncoding(parameters, _data);
            var verifier = new MLDsaSigner(parameters, true);
            verifier.Init(false, keyParams);
            verifier.BlockUpdate(message, 0, message.Length);
            return verifier.VerifySignature(signature.AsBytes());
        }
        catch
        {
            return false;
        }
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<MLDSAPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(MLDSAPublicKey? other)
    {
        if (other is null) return false;
        return Level == other.Level && _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MLDSAPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Level);
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two MLDSAPublicKey instances.</summary>
    public static bool operator ==(MLDSAPublicKey? left, MLDSAPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two MLDSAPublicKey instances.</summary>
    public static bool operator !=(MLDSAPublicKey? left, MLDSAPublicKey? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagMldsaPublicKey);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation as an array [level, byte_string].
    /// </summary>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Level.ToCbor(),
            Cbor.ToByteString(_data),
        };
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="MLDSAPublicKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be an array of [level, byte_string]).</param>
    /// <returns>A new <see cref="MLDSAPublicKey"/>.</returns>
    public static MLDSAPublicKey FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("MLDSAPublicKey", "must have exactly 2 elements");
        var level = MLDSALevelExtensions.MLDSALevelFromCbor(elements[0]);
        var data = elements[1].TryIntoByteString();
        return FromBytes(level, data);
    }

    /// <summary>
    /// Decodes an <see cref="MLDSAPublicKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="MLDSAPublicKey"/>.</returns>
    public static MLDSAPublicKey FromTaggedCbor(Cbor cbor)
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

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"MLDSAPublicKey({Level}, {((IReferenceProvider)this).RefHexShort()})";
}
