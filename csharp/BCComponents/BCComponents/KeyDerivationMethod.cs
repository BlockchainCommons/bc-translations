namespace BlockchainCommons.BCComponents;

/// <summary>
/// Supported key derivation methods.
/// </summary>
/// <remarks>
/// CDDL:
/// <code>
/// KeyDerivationMethod = HKDF / PBKDF2 / Scrypt / Argon2id
/// HKDF = 0
/// PBKDF2 = 1
/// Scrypt = 2
/// Argon2id = 3
/// </code>
/// </remarks>
public enum KeyDerivationMethod
{
    /// <summary>HKDF key derivation.</summary>
    HKDF = 0,

    /// <summary>PBKDF2 key derivation.</summary>
    PBKDF2 = 1,

    /// <summary>Scrypt key derivation.</summary>
    Scrypt = 2,

    /// <summary>Argon2id key derivation.</summary>
    Argon2id = 3,
}

/// <summary>
/// Extension methods for <see cref="KeyDerivationMethod"/>.
/// </summary>
public static class KeyDerivationMethodExtensions
{
    /// <summary>
    /// Returns the <see cref="KeyDerivationMethod"/> for the given index,
    /// or <c>null</c> if the index is not recognized.
    /// </summary>
    public static KeyDerivationMethod? FromIndex(int index) => index switch
    {
        0 => KeyDerivationMethod.HKDF,
        1 => KeyDerivationMethod.PBKDF2,
        2 => KeyDerivationMethod.Scrypt,
        3 => KeyDerivationMethod.Argon2id,
        _ => null,
    };
}
