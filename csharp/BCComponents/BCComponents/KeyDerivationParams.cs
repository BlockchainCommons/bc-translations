using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Wrapper for the different key derivation parameter types.
/// </summary>
/// <remarks>
/// Each variant holds the parameters for a specific key derivation method.
/// This type is used internally by <see cref="EncryptedKey"/> to dispatch
/// lock/unlock operations to the appropriate implementation.
/// </remarks>
public sealed class KeyDerivationParams : IEquatable<KeyDerivationParams>
{
    private enum Variant { HKDF, PBKDF2, Scrypt, Argon2id }

    private readonly Variant _variant;
    private readonly HKDFParams? _hkdf;
    private readonly PBKDF2Params? _pbkdf2;
    private readonly ScryptParams? _scrypt;
    private readonly Argon2idParams? _argon2id;

    private KeyDerivationParams(
        Variant variant,
        HKDFParams? hkdf,
        PBKDF2Params? pbkdf2,
        ScryptParams? scrypt,
        Argon2idParams? argon2id)
    {
        _variant = variant;
        _hkdf = hkdf;
        _pbkdf2 = pbkdf2;
        _scrypt = scrypt;
        _argon2id = argon2id;
    }

    /// <summary>Creates HKDF key derivation parameters.</summary>
    public static KeyDerivationParams FromHKDF(HKDFParams p) => new(Variant.HKDF, p, null, null, null);

    /// <summary>Creates PBKDF2 key derivation parameters.</summary>
    public static KeyDerivationParams FromPBKDF2(PBKDF2Params p) => new(Variant.PBKDF2, null, p, null, null);

    /// <summary>Creates Scrypt key derivation parameters.</summary>
    public static KeyDerivationParams FromScrypt(ScryptParams p) => new(Variant.Scrypt, null, null, p, null);

    /// <summary>Creates Argon2id key derivation parameters.</summary>
    public static KeyDerivationParams FromArgon2id(Argon2idParams p) => new(Variant.Argon2id, null, null, null, p);

    /// <summary>Returns the <see cref="KeyDerivationMethod"/> for these parameters.</summary>
    public KeyDerivationMethod Method => _variant switch
    {
        Variant.HKDF => KeyDerivationMethod.HKDF,
        Variant.PBKDF2 => KeyDerivationMethod.PBKDF2,
        Variant.Scrypt => KeyDerivationMethod.Scrypt,
        Variant.Argon2id => KeyDerivationMethod.Argon2id,
        _ => throw new InvalidOperationException(),
    };

    /// <summary>Returns <c>true</c> if the derivation method is password-based.</summary>
    public bool IsPasswordBased => _variant is Variant.PBKDF2 or Variant.Scrypt or Variant.Argon2id;

    /// <summary>Returns <c>true</c> if the derivation method is SSH agent-based.</summary>
    public bool IsSshAgent => false;

    /// <summary>
    /// Derives a key from <paramref name="secret"/> and encrypts
    /// <paramref name="contentKey"/> with it.
    /// </summary>
    public EncryptedMessage Lock(SymmetricKey contentKey, byte[] secret) => _variant switch
    {
        Variant.HKDF => _hkdf!.Lock(contentKey, secret),
        Variant.PBKDF2 => _pbkdf2!.Lock(contentKey, secret),
        Variant.Scrypt => _scrypt!.Lock(contentKey, secret),
        Variant.Argon2id => _argon2id!.Lock(contentKey, secret),
        _ => throw new InvalidOperationException(),
    };

    /// <summary>Encodes these parameters as CBOR.</summary>
    public Cbor ToCbor() => _variant switch
    {
        Variant.HKDF => _hkdf!.ToCbor(),
        Variant.PBKDF2 => _pbkdf2!.ToCbor(),
        Variant.Scrypt => _scrypt!.ToCbor(),
        Variant.Argon2id => _argon2id!.ToCbor(),
        _ => throw new InvalidOperationException(),
    };

    /// <summary>
    /// Decodes <see cref="KeyDerivationParams"/> from a CBOR array.
    /// </summary>
    /// <param name="cbor">The CBOR array value.</param>
    /// <returns>A new <see cref="KeyDerivationParams"/>.</returns>
    public static KeyDerivationParams FromCbor(Cbor cbor)
    {
        var a = cbor.TryIntoArray();
        if (a.Count == 0)
            throw BCComponentsException.General("Empty KeyDerivationParams array");

        var index = (int)a[0].TryIntoUInt64();
        var method = KeyDerivationMethodExtensions.FromIndex(index)
            ?? throw BCComponentsException.General($"Invalid KeyDerivationMethod index: {index}");

        return method switch
        {
            KeyDerivationMethod.HKDF => FromHKDF(HKDFParams.FromCbor(cbor)),
            KeyDerivationMethod.PBKDF2 => FromPBKDF2(PBKDF2Params.FromCbor(cbor)),
            KeyDerivationMethod.Scrypt => FromScrypt(ScryptParams.FromCbor(cbor)),
            KeyDerivationMethod.Argon2id => FromArgon2id(Argon2idParams.FromCbor(cbor)),
            _ => throw new InvalidOperationException(),
        };
    }

    /// <inheritdoc/>
    public override string ToString() => _variant switch
    {
        Variant.HKDF => _hkdf!.ToString(),
        Variant.PBKDF2 => _pbkdf2!.ToString(),
        Variant.Scrypt => _scrypt!.ToString(),
        Variant.Argon2id => _argon2id!.ToString(),
        _ => "Unknown",
    };

    /// <inheritdoc/>
    public bool Equals(KeyDerivationParams? other)
    {
        if (other is null) return false;
        if (_variant != other._variant) return false;
        return _variant switch
        {
            Variant.HKDF => _hkdf!.Equals(other._hkdf),
            Variant.PBKDF2 => _pbkdf2!.Equals(other._pbkdf2),
            Variant.Scrypt => _scrypt!.Equals(other._scrypt),
            Variant.Argon2id => _argon2id!.Equals(other._argon2id),
            _ => false,
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is KeyDerivationParams p && Equals(p);

    /// <inheritdoc/>
    public override int GetHashCode() => _variant switch
    {
        Variant.HKDF => HashCode.Combine(_variant, _hkdf),
        Variant.PBKDF2 => HashCode.Combine(_variant, _pbkdf2),
        Variant.Scrypt => HashCode.Combine(_variant, _scrypt),
        Variant.Argon2id => HashCode.Combine(_variant, _argon2id),
        _ => 0,
    };
}
