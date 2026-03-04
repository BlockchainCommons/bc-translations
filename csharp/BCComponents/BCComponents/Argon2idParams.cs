using BlockchainCommons.BCCrypto;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Argon2id-based key derivation parameters.
/// </summary>
/// <remarks>
/// CDDL:
/// <code>
/// Argon2idParams = [3, Salt]
/// </code>
/// </remarks>
public sealed class Argon2idParams : IEquatable<Argon2idParams>, IKeyDerivation
{
    /// <summary>Default salt length in bytes.</summary>
    public const int SaltLen = 16;

    /// <summary>Gets the salt used for key derivation.</summary>
    public Salt Salt { get; }

    /// <summary>
    /// Creates Argon2id parameters with the given salt.
    /// </summary>
    /// <param name="salt">The salt for key derivation.</param>
    public Argon2idParams(Salt salt)
    {
        Salt = salt;
    }

    /// <summary>
    /// Creates default Argon2id parameters with a random 16-byte salt.
    /// </summary>
    public Argon2idParams()
        : this(Salt.CreateWithLength(SaltLen))
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
        var derived = ArgonKdf.Argon2Id(secret, Salt.AsBytes(), 32);
        return SymmetricKey.FromData(derived);
    }

    /// <summary>Encodes these parameters as a CBOR array.</summary>
    public Cbor ToCbor() => Cbor.FromList(new List<Cbor>
    {
        Cbor.FromInt((int)KeyDerivationMethod.Argon2id),
        Salt.UntaggedCbor(),
    });

    /// <summary>Decodes <see cref="Argon2idParams"/> from a CBOR array.</summary>
    /// <param name="cbor">The CBOR array value.</param>
    /// <returns>A new <see cref="Argon2idParams"/>.</returns>
    public static Argon2idParams FromCbor(Cbor cbor)
    {
        var a = cbor.TryIntoArray();
        if (a.Count != 2)
            throw BCComponentsException.General($"Invalid Argon2idParams: expected 2 elements, got {a.Count}");
        var salt = Salt.FromUntaggedCbor(a[1]);
        return new Argon2idParams(salt);
    }

    /// <inheritdoc/>
    public override string ToString() => "Argon2id";

    /// <inheritdoc/>
    public bool Equals(Argon2idParams? other)
    {
        if (other is null) return false;
        return Salt == other.Salt;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Argon2idParams p && Equals(p);

    /// <inheritdoc/>
    public override int GetHashCode() => Salt.GetHashCode();
}
