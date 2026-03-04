using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Supported key encapsulation mechanisms.
/// </summary>
/// <remarks>
/// <para>
/// Key Encapsulation Mechanisms (KEMs) are cryptographic algorithms designed to
/// securely establish a shared secret between parties in public-key cryptography.
/// </para>
/// <para>
/// This enum represents the various KEM schemes supported:
/// <list type="bullet">
/// <item><see cref="X25519"/>: A Diffie-Hellman key exchange mechanism using the
/// Curve25519 elliptic curve</item>
/// <item><see cref="MLKEM512"/>: ML-KEM at NIST security level 1</item>
/// <item><see cref="MLKEM768"/>: ML-KEM at NIST security level 3</item>
/// <item><see cref="MLKEM1024"/>: ML-KEM at NIST security level 5</item>
/// </list>
/// </para>
/// </remarks>
public enum EncapsulationScheme
{
    /// <summary>X25519 key agreement (default).</summary>
    X25519 = 0,

    /// <summary>ML-KEM-512 post-quantum key encapsulation (NIST level 1).</summary>
    MLKEM512,

    /// <summary>ML-KEM-768 post-quantum key encapsulation (NIST level 3).</summary>
    MLKEM768,

    /// <summary>ML-KEM-1024 post-quantum key encapsulation (NIST level 5).</summary>
    MLKEM1024,
}

/// <summary>
/// Extension methods for <see cref="EncapsulationScheme"/>.
/// </summary>
public static class EncapsulationSchemeExtensions
{
    /// <summary>
    /// Generates a new random key pair for the specified encapsulation scheme.
    /// </summary>
    /// <param name="scheme">The encapsulation scheme.</param>
    /// <returns>
    /// A tuple containing the <see cref="EncapsulationPrivateKey"/> and
    /// <see cref="EncapsulationPublicKey"/>.
    /// </returns>
    public static (EncapsulationPrivateKey PrivateKey, EncapsulationPublicKey PublicKey) Keypair(
        this EncapsulationScheme scheme)
    {
        return scheme switch
        {
            EncapsulationScheme.X25519 =>
                WrapX25519(X25519PrivateKey.Keypair()),
            EncapsulationScheme.MLKEM512 =>
                WrapMLKEM(MLKEMLevel.MLKEM512.Keypair()),
            EncapsulationScheme.MLKEM768 =>
                WrapMLKEM(MLKEMLevel.MLKEM768.Keypair()),
            EncapsulationScheme.MLKEM1024 =>
                WrapMLKEM(MLKEMLevel.MLKEM1024.Keypair()),
            _ => throw new ArgumentOutOfRangeException(nameof(scheme)),
        };
    }

    /// <summary>
    /// Generates a deterministic key pair using the provided random number generator.
    /// </summary>
    /// <remarks>
    /// Only X25519 supports deterministic key generation. ML-KEM schemes will
    /// throw a <see cref="BCComponentsException"/>.
    /// </remarks>
    /// <param name="scheme">The encapsulation scheme.</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>
    /// A tuple containing the <see cref="EncapsulationPrivateKey"/> and
    /// <see cref="EncapsulationPublicKey"/>.
    /// </returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if deterministic key generation is not supported for the selected scheme.
    /// </exception>
    public static (EncapsulationPrivateKey PrivateKey, EncapsulationPublicKey PublicKey) KeypairUsing(
        this EncapsulationScheme scheme,
        IRandomNumberGenerator rng)
    {
        return scheme switch
        {
            EncapsulationScheme.X25519 =>
                WrapX25519(X25519PrivateKey.KeypairUsing(rng)),
            _ => throw BCComponentsException.General(
                "Deterministic keypair generation not supported for this encapsulation scheme"),
        };
    }

    private static (EncapsulationPrivateKey, EncapsulationPublicKey) WrapX25519(
        (X25519PrivateKey PrivateKey, X25519PublicKey PublicKey) kp)
    {
        return (
            EncapsulationPrivateKey.FromX25519(kp.PrivateKey),
            EncapsulationPublicKey.FromX25519(kp.PublicKey)
        );
    }

    private static (EncapsulationPrivateKey, EncapsulationPublicKey) WrapMLKEM(
        (MLKEMPrivateKey PrivateKey, MLKEMPublicKey PublicKey) kp)
    {
        return (
            EncapsulationPrivateKey.FromMLKEM(kp.PrivateKey),
            EncapsulationPublicKey.FromMLKEM(kp.PublicKey)
        );
    }
}
