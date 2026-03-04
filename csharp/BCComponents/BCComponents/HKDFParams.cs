using BlockchainCommons.BCCrypto;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// HKDF-based key derivation parameters.
/// </summary>
/// <remarks>
/// CDDL:
/// <code>
/// HKDFParams = [0, Salt, HashType]
/// </code>
/// </remarks>
public sealed class HKDFParams : IEquatable<HKDFParams>, IKeyDerivation
{
    /// <summary>Default salt length in bytes.</summary>
    public const int SaltLen = 16;

    /// <summary>Gets the salt used for key derivation.</summary>
    public Salt Salt { get; }

    /// <summary>Gets the hash algorithm.</summary>
    public HashType HashType { get; }

    /// <summary>
    /// Creates HKDF parameters with the given salt and hash type.
    /// </summary>
    /// <param name="salt">The salt for key derivation.</param>
    /// <param name="hashType">The hash algorithm (default: SHA-256).</param>
    public HKDFParams(Salt salt, HashType hashType = HashType.SHA256)
    {
        Salt = salt;
        HashType = hashType;
    }

    /// <summary>
    /// Creates default HKDF parameters with a random 16-byte salt and SHA-256.
    /// </summary>
    public HKDFParams()
        : this(Salt.CreateWithLength(SaltLen), HashType.SHA256)
    {
    }

    /// <inheritdoc/>
    public EncryptedMessage Lock(SymmetricKey contentKey, byte[] secret)
    {
        var derivedKey = DeriveKey(secret);
        var encodedMethod = ToCbor().ToCborData();
        return derivedKey.Encrypt(contentKey.Data, encodedMethod, null);
    }

    /// <inheritdoc/>
    public SymmetricKey Unlock(EncryptedMessage encryptedMessage, byte[] secret)
    {
        var derivedKey = DeriveKey(secret);
        var decrypted = derivedKey.Decrypt(encryptedMessage);
        return SymmetricKey.FromData(decrypted);
    }

    private SymmetricKey DeriveKey(byte[] secret)
    {
        var derived = HashType switch
        {
            HashType.SHA256 => Hash.HkdfHmacSha256(secret, Salt.AsBytes(), 32),
            HashType.SHA512 => Hash.HkdfHmacSha512(secret, Salt.AsBytes(), 32),
            _ => throw BCComponentsException.General($"Unknown hash type: {HashType}"),
        };
        return SymmetricKey.FromData(derived);
    }

    /// <summary>Encodes these parameters as a CBOR array.</summary>
    public Cbor ToCbor() => Cbor.FromList(new List<Cbor>
    {
        Cbor.FromInt((int)KeyDerivationMethod.HKDF),
        Salt.UntaggedCbor(),
        HashType.ToCbor(),
    });

    /// <summary>Decodes <see cref="HKDFParams"/> from a CBOR array.</summary>
    /// <param name="cbor">The CBOR array value.</param>
    /// <returns>A new <see cref="HKDFParams"/>.</returns>
    public static HKDFParams FromCbor(Cbor cbor)
    {
        var a = cbor.TryIntoArray();
        if (a.Count != 3)
            throw BCComponentsException.General($"Invalid HKDFParams: expected 3 elements, got {a.Count}");
        var salt = Salt.FromUntaggedCbor(a[1]);
        var hashType = HashTypeExtensions.FromCbor(a[2]);
        return new HKDFParams(salt, hashType);
    }

    /// <inheritdoc/>
    public override string ToString() => $"HKDF({HashType})";

    /// <inheritdoc/>
    public bool Equals(HKDFParams? other)
    {
        if (other is null) return false;
        return Salt == other.Salt && HashType == other.HashType;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is HKDFParams p && Equals(p);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Salt, HashType);
}
