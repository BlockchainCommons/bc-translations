using BlockchainCommons.BCCrypto;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Scrypt-based key derivation parameters.
/// </summary>
/// <remarks>
/// CDDL:
/// <code>
/// ScryptParams = [2, Salt, log_n: uint, r: uint, p: uint]
/// </code>
/// </remarks>
public sealed class ScryptParams : IEquatable<ScryptParams>, IKeyDerivation
{
    /// <summary>Default salt length in bytes.</summary>
    public const int SaltLen = 16;

    /// <summary>Default log2 of the CPU/memory cost parameter.</summary>
    public const int DefaultLogN = 15;

    /// <summary>Default block size parameter.</summary>
    public const int DefaultR = 8;

    /// <summary>Default parallelisation parameter.</summary>
    public const int DefaultP = 1;

    /// <summary>Gets the salt used for key derivation.</summary>
    public Salt Salt { get; }

    /// <summary>Gets the log2 of the CPU/memory cost parameter.</summary>
    public int LogN { get; }

    /// <summary>Gets the block size parameter.</summary>
    public int R { get; }

    /// <summary>Gets the parallelisation parameter.</summary>
    public int P { get; }

    /// <summary>
    /// Creates Scrypt parameters with the given values.
    /// </summary>
    /// <param name="salt">The salt for key derivation.</param>
    /// <param name="logN">The log2 of the CPU/memory cost parameter.</param>
    /// <param name="r">The block size parameter.</param>
    /// <param name="p">The parallelisation parameter.</param>
    public ScryptParams(Salt salt, int logN = DefaultLogN, int r = DefaultR, int p = DefaultP)
    {
        Salt = salt;
        LogN = logN;
        R = r;
        P = p;
    }

    /// <summary>
    /// Creates default Scrypt parameters with a random 16-byte salt and standard
    /// cost factors.
    /// </summary>
    public ScryptParams()
        : this(Salt.CreateWithLength(SaltLen), DefaultLogN, DefaultR, DefaultP)
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
        var derived = ScryptKdf.DeriveOpt(secret, Salt.AsBytes(), 32, (byte)LogN, (uint)R, (uint)P);
        return SymmetricKey.FromData(derived);
    }

    /// <summary>Encodes these parameters as a CBOR array.</summary>
    public Cbor ToCbor() => Cbor.FromList(new List<Cbor>
    {
        Cbor.FromInt((int)KeyDerivationMethod.Scrypt),
        Salt.UntaggedCbor(),
        Cbor.FromInt(LogN),
        Cbor.FromInt(R),
        Cbor.FromInt(P),
    });

    /// <summary>Decodes <see cref="ScryptParams"/> from a CBOR array.</summary>
    /// <param name="cbor">The CBOR array value.</param>
    /// <returns>A new <see cref="ScryptParams"/>.</returns>
    public static ScryptParams FromCbor(Cbor cbor)
    {
        var a = cbor.TryIntoArray();
        if (a.Count != 5)
            throw BCComponentsException.General($"Invalid ScryptParams: expected 5 elements, got {a.Count}");
        var salt = Salt.FromUntaggedCbor(a[1]);
        var logN = (int)a[2].TryIntoUInt64();
        var r = (int)a[3].TryIntoUInt64();
        var p = (int)a[4].TryIntoUInt64();
        return new ScryptParams(salt, logN, r, p);
    }

    /// <inheritdoc/>
    public override string ToString() => "Scrypt";

    /// <inheritdoc/>
    public bool Equals(ScryptParams? other)
    {
        if (other is null) return false;
        return Salt == other.Salt
            && LogN == other.LogN
            && R == other.R
            && P == other.P;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ScryptParams p && Equals(p);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Salt, LogN, R, P);
}
