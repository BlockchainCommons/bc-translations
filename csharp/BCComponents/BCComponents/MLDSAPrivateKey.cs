using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A private key for the ML-DSA post-quantum digital signature algorithm.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MLDSAPrivateKey"/> represents a private key that can create digital
/// signatures using the ML-DSA (Module Lattice-based Digital Signature Algorithm)
/// post-quantum algorithm. It supports multiple security levels:
/// </para>
/// <list type="bullet">
/// <item><see cref="MLDSALevel.MLDSA44"/>: NIST security level 2</item>
/// <item><see cref="MLDSALevel.MLDSA65"/>: NIST security level 3</item>
/// <item><see cref="MLDSALevel.MLDSA87"/>: NIST security level 5</item>
/// </list>
/// </remarks>
public sealed class MLDSAPrivateKey
    : IEquatable<MLDSAPrivateKey>,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      IReferenceProvider
{
    private readonly byte[] _data;

    /// <summary>The security level of this private key.</summary>
    public MLDSALevel Level { get; }

    private MLDSAPrivateKey(MLDSALevel level, byte[] data)
    {
        Level = level;
        _data = data;
    }

    /// <summary>
    /// Creates an ML-DSA private key from raw bytes and a security level.
    /// </summary>
    /// <param name="level">The security level of the key.</param>
    /// <param name="data">The raw key bytes.</param>
    /// <returns>A new <see cref="MLDSAPrivateKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the data length does not match the expected key size for the level.
    /// </exception>
    public static MLDSAPrivateKey FromBytes(MLDSALevel level, byte[] data)
    {
        var expectedSize = level.PrivateKeySize();
        if (data.Length != expectedSize)
            throw BCComponentsException.InvalidSize($"ML-DSA {level} private key", expectedSize, data.Length);
        var copy = new byte[data.Length];
        Array.Copy(data, copy, data.Length);
        return new MLDSAPrivateKey(level, copy);
    }

    /// <summary>Returns the raw key bytes.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Returns the raw key bytes as a read-only span.</summary>
    internal ReadOnlySpan<byte> AsBytesSpan() => _data;

    /// <summary>Returns the size of this key in bytes.</summary>
    public int Size => _data.Length;

    /// <summary>
    /// Signs a message using this ML-DSA private key.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>An <see cref="MLDSASignature"/> for the message.</returns>
    public MLDSASignature Sign(byte[] message)
    {
        var parameters = GetParameters(Level);
        var keyParams = MLDsaPrivateKeyParameters.FromEncoding(parameters, _data);
        var signer = new MLDsaSigner(parameters, true);
        signer.Init(true, keyParams);
        signer.BlockUpdate(message, 0, message.Length);
        var sigBytes = signer.GenerateSignature();
        return MLDSASignature.FromBytes(Level, sigBytes);
    }

    /// <summary>
    /// Generates a new ML-DSA keypair for the specified security level.
    /// </summary>
    /// <param name="level">The security level.</param>
    /// <returns>A tuple containing the private key and public key.</returns>
    internal static (MLDSAPrivateKey PrivateKey, MLDSAPublicKey PublicKey) GenerateKeypair(MLDSALevel level)
    {
        var parameters = GetParameters(level);
        var keyGenParams = new MLDsaKeyGenerationParameters(new SecureRandom(), parameters);
        var keyGen = new MLDsaKeyPairGenerator();
        keyGen.Init(keyGenParams);
        var keyPair = keyGen.GenerateKeyPair();

        var privParams = (MLDsaPrivateKeyParameters)keyPair.Private;
        var pubParams = (MLDsaPublicKeyParameters)keyPair.Public;

        var privateKey = new MLDSAPrivateKey(level, privParams.GetEncoded());
        var publicKey = MLDSAPublicKey.FromBytes(level, pubParams.GetEncoded());

        return (privateKey, publicKey);
    }

    /// <summary>
    /// Returns the BouncyCastle parameter set for the given level.
    /// </summary>
    internal static MLDsaParameters GetParameters(MLDSALevel level)
    {
        return level switch
        {
            MLDSALevel.MLDSA44 => MLDsaParameters.ml_dsa_44,
            MLDSALevel.MLDSA65 => MLDsaParameters.ml_dsa_65,
            MLDSALevel.MLDSA87 => MLDsaParameters.ml_dsa_87,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<MLDSAPrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(MLDSAPrivateKey? other)
    {
        if (other is null) return false;
        return Level == other.Level && _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MLDSAPrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Level);
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two MLDSAPrivateKey instances.</summary>
    public static bool operator ==(MLDSAPrivateKey? left, MLDSAPrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two MLDSAPrivateKey instances.</summary>
    public static bool operator !=(MLDSAPrivateKey? left, MLDSAPrivateKey? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagMldsaPrivateKey);

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
    /// Decodes an <see cref="MLDSAPrivateKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be an array of [level, byte_string]).</param>
    /// <returns>A new <see cref="MLDSAPrivateKey"/>.</returns>
    public static MLDSAPrivateKey FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count != 2)
            throw BCComponentsException.InvalidData("MLDSAPrivateKey", "must have exactly 2 elements");
        var level = MLDSALevelExtensions.MLDSALevelFromCbor(elements[0]);
        var data = elements[1].TryIntoByteString();
        return FromBytes(level, data);
    }

    /// <summary>
    /// Decodes an <see cref="MLDSAPrivateKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="MLDSAPrivateKey"/>.</returns>
    public static MLDSAPrivateKey FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => $"MLDSAPrivateKey({Level}, {((IReferenceProvider)this).RefHexShort()})";
}
