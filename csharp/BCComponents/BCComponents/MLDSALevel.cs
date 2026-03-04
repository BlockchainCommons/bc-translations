using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Security levels for the ML-DSA post-quantum digital signature algorithm.
/// </summary>
/// <remarks>
/// <para>
/// ML-DSA (Module Lattice-based Digital Signature Algorithm) is a post-quantum
/// digital signature algorithm standardized by NIST. It provides resistance
/// against attacks from both classical and quantum computers.
/// </para>
/// <para>
/// Each security level offers different trade-offs between security,
/// performance, and key/signature sizes:
/// </para>
/// <list type="bullet">
/// <item><see cref="MLDSA44"/>: NIST security level 2 (roughly equivalent to AES-128)</item>
/// <item><see cref="MLDSA65"/>: NIST security level 3 (roughly equivalent to AES-192)</item>
/// <item><see cref="MLDSA87"/>: NIST security level 5 (roughly equivalent to AES-256)</item>
/// </list>
/// <para>
/// The numeric CBOR encoding values (2, 3, 5) correspond to the NIST security levels.
/// </para>
/// </remarks>
public enum MLDSALevel
{
    /// <summary>ML-DSA Level 2 (NIST security level 2, roughly equivalent to AES-128).</summary>
    MLDSA44 = 2,

    /// <summary>ML-DSA Level 3 (NIST security level 3, roughly equivalent to AES-192).</summary>
    MLDSA65 = 3,

    /// <summary>ML-DSA Level 5 (NIST security level 5, roughly equivalent to AES-256).</summary>
    MLDSA87 = 5,
}

/// <summary>
/// Extension methods for <see cref="MLDSALevel"/>.
/// </summary>
public static class MLDSALevelExtensions
{
    /// <summary>
    /// Generates a new ML-DSA keypair for the specified security level.
    /// </summary>
    /// <param name="level">The ML-DSA security level.</param>
    /// <returns>A tuple containing the private key and public key.</returns>
    public static (MLDSAPrivateKey PrivateKey, MLDSAPublicKey PublicKey) Keypair(this MLDSALevel level)
    {
        return MLDSAPrivateKey.GenerateKeypair(level);
    }

    /// <summary>
    /// Converts the ML-DSA level to its CBOR representation (numeric: 2, 3, or 5).
    /// </summary>
    /// <param name="level">The ML-DSA level.</param>
    /// <returns>The CBOR value.</returns>
    public static Cbor ToCbor(this MLDSALevel level)
    {
        return Cbor.FromInt((int)level);
    }

    /// <summary>
    /// Parses an ML-DSA level from a CBOR value.
    /// </summary>
    /// <param name="cbor">The CBOR value (must be unsigned 2, 3, or 5).</param>
    /// <returns>The corresponding ML-DSA level.</returns>
    /// <exception cref="BCComponentsException">Thrown if the CBOR value is not a valid level.</exception>
    public static MLDSALevel MLDSALevelFromCbor(Cbor cbor)
    {
        var value = (int)cbor.TryIntoUInt64();
        return value switch
        {
            2 => MLDSALevel.MLDSA44,
            3 => MLDSALevel.MLDSA65,
            5 => MLDSALevel.MLDSA87,
            _ => throw BCComponentsException.PostQuantum($"Invalid ML-DSA level: {value}"),
        };
    }

    /// <summary>
    /// Returns the size of a private key in bytes for the specified security level.
    /// </summary>
    /// <param name="level">The ML-DSA security level.</param>
    /// <returns>The private key size in bytes.</returns>
    public static int PrivateKeySize(this MLDSALevel level)
    {
        return level switch
        {
            MLDSALevel.MLDSA44 => 2560,
            MLDSALevel.MLDSA65 => 4032,
            MLDSALevel.MLDSA87 => 4896,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    /// <summary>
    /// Returns the size of a public key in bytes for the specified security level.
    /// </summary>
    /// <param name="level">The ML-DSA security level.</param>
    /// <returns>The public key size in bytes.</returns>
    public static int PublicKeySize(this MLDSALevel level)
    {
        return level switch
        {
            MLDSALevel.MLDSA44 => 1312,
            MLDSALevel.MLDSA65 => 1952,
            MLDSALevel.MLDSA87 => 2592,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    /// <summary>
    /// Returns the size of a signature in bytes for the specified security level.
    /// </summary>
    /// <param name="level">The ML-DSA security level.</param>
    /// <returns>The signature size in bytes.</returns>
    public static int SignatureSize(this MLDSALevel level)
    {
        return level switch
        {
            MLDSALevel.MLDSA44 => 2420,
            MLDSALevel.MLDSA65 => 3309,
            MLDSALevel.MLDSA87 => 4627,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }
}
