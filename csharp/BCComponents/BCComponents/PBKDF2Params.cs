using BlockchainCommons.BCCrypto;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// PBKDF2-based key derivation parameters.
/// </summary>
/// <remarks>
/// CDDL:
/// <code>
/// PBKDF2Params = [1, Salt, iterations: uint, HashType]
/// </code>
/// </remarks>
public sealed class PBKDF2Params : IEquatable<PBKDF2Params>, IKeyDerivation
{
    /// <summary>Default salt length in bytes.</summary>
    public const int SaltLen = 16;

    /// <summary>Default number of PBKDF2 iterations.</summary>
    public const int DefaultIterations = 100_000;

    /// <summary>Gets the salt used for key derivation.</summary>
    public Salt Salt { get; }

    /// <summary>Gets the number of PBKDF2 iterations.</summary>
    public int Iterations { get; }

    /// <summary>Gets the hash algorithm.</summary>
    public HashType HashType { get; }

    /// <summary>
    /// Creates PBKDF2 parameters with the given values.
    /// </summary>
    /// <param name="salt">The salt for key derivation.</param>
    /// <param name="iterations">The number of iterations.</param>
    /// <param name="hashType">The hash algorithm (default: SHA-256).</param>
    public PBKDF2Params(Salt salt, int iterations = DefaultIterations, HashType hashType = HashType.SHA256)
    {
        Salt = salt;
        Iterations = iterations;
        HashType = hashType;
    }

    /// <summary>
    /// Creates default PBKDF2 parameters with a random 16-byte salt, 100,000 iterations,
    /// and SHA-256.
    /// </summary>
    public PBKDF2Params()
        : this(Salt.CreateWithLength(SaltLen), DefaultIterations, HashType.SHA256)
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
            HashType.SHA256 => Hash.Pbkdf2HmacSha256(secret, Salt.AsBytes(), (uint)Iterations, 32),
            HashType.SHA512 => Hash.Pbkdf2HmacSha512(secret, Salt.AsBytes(), (uint)Iterations, 32),
            _ => throw BCComponentsException.General($"Unknown hash type: {HashType}"),
        };
        return SymmetricKey.FromData(derived);
    }

    /// <summary>Encodes these parameters as a CBOR array.</summary>
    public Cbor ToCbor() => Cbor.FromList(new List<Cbor>
    {
        Cbor.FromInt((int)KeyDerivationMethod.PBKDF2),
        Salt.UntaggedCbor(),
        Cbor.FromInt(Iterations),
        HashType.ToCbor(),
    });

    /// <summary>Decodes <see cref="PBKDF2Params"/> from a CBOR array.</summary>
    /// <param name="cbor">The CBOR array value.</param>
    /// <returns>A new <see cref="PBKDF2Params"/>.</returns>
    public static PBKDF2Params FromCbor(Cbor cbor)
    {
        var a = cbor.TryIntoArray();
        if (a.Count != 4)
            throw BCComponentsException.General($"Invalid PBKDF2Params: expected 4 elements, got {a.Count}");
        var salt = Salt.FromUntaggedCbor(a[1]);
        var iterations = (int)a[2].TryIntoUInt64();
        var hashType = HashTypeExtensions.FromCbor(a[3]);
        return new PBKDF2Params(salt, iterations, hashType);
    }

    /// <inheritdoc/>
    public override string ToString() => $"PBKDF2({HashType})";

    /// <inheritdoc/>
    public bool Equals(PBKDF2Params? other)
    {
        if (other is null) return false;
        return Salt == other.Salt
            && Iterations == other.Iterations
            && HashType == other.HashType;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PBKDF2Params p && Equals(p);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Salt, Iterations, HashType);
}
