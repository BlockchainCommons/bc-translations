using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Security levels for the ML-KEM post-quantum key encapsulation mechanism.
/// </summary>
/// <remarks>
/// <para>
/// ML-KEM (Module Lattice-based Key Encapsulation Mechanism) is a post-quantum
/// key encapsulation mechanism standardized by NIST. It provides resistance
/// against attacks from both classical and quantum computers.
/// </para>
/// <para>
/// Each security level offers different trade-offs between security,
/// performance, and key/ciphertext sizes:
/// <list type="bullet">
/// <item><see cref="MLKEM512"/>: NIST security level 1 (roughly equivalent to AES-128)</item>
/// <item><see cref="MLKEM768"/>: NIST security level 3 (roughly equivalent to AES-192)</item>
/// <item><see cref="MLKEM1024"/>: NIST security level 5 (roughly equivalent to AES-256)</item>
/// </list>
/// </para>
/// </remarks>
public enum MLKEMLevel
{
    /// <summary>ML-KEM-512 (NIST security level 1, roughly equivalent to AES-128).</summary>
    MLKEM512 = 512,

    /// <summary>ML-KEM-768 (NIST security level 3, roughly equivalent to AES-192).</summary>
    MLKEM768 = 768,

    /// <summary>ML-KEM-1024 (NIST security level 5, roughly equivalent to AES-256).</summary>
    MLKEM1024 = 1024,
}

/// <summary>
/// Extension methods for <see cref="MLKEMLevel"/>.
/// </summary>
public static class MLKEMLevelExtensions
{
    /// <summary>The size of a shared secret in bytes (32 bytes for all security levels).</summary>
    public const int SharedSecretSize = 32;

    /// <summary>
    /// Returns the BouncyCastle ML-KEM parameters for this security level.
    /// </summary>
    /// <param name="level">The ML-KEM security level.</param>
    /// <returns>The corresponding <see cref="MLKemParameters"/>.</returns>
    public static MLKemParameters Parameters(this MLKEMLevel level)
    {
        return level switch
        {
            MLKEMLevel.MLKEM512 => MLKemParameters.ml_kem_512,
            MLKEMLevel.MLKEM768 => MLKemParameters.ml_kem_768,
            MLKEMLevel.MLKEM1024 => MLKemParameters.ml_kem_1024,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    /// <summary>
    /// Generates a new ML-KEM keypair with the specified security level.
    /// </summary>
    /// <param name="level">The ML-KEM security level.</param>
    /// <returns>A tuple containing the private key and public key.</returns>
    public static (MLKEMPrivateKey PrivateKey, MLKEMPublicKey PublicKey) Keypair(
        this MLKEMLevel level)
    {
        var kpGen = new MLKemKeyPairGenerator();
        kpGen.Init(new MLKemKeyGenerationParameters(
            new Org.BouncyCastle.Security.SecureRandom(), level.Parameters()));
        var kp = kpGen.GenerateKeyPair();

        var privParams = (MLKemPrivateKeyParameters)kp.Private;
        var pubParams = (MLKemPublicKeyParameters)kp.Public;

        return (
            MLKEMPrivateKey.FromParameters(level, privParams),
            MLKEMPublicKey.FromParameters(level, pubParams)
        );
    }

    /// <summary>
    /// Returns the size of a private key in bytes for this security level.
    /// </summary>
    /// <param name="level">The ML-KEM security level.</param>
    /// <returns>The private key size in bytes.</returns>
    public static int PrivateKeySize(this MLKEMLevel level)
    {
        return level switch
        {
            MLKEMLevel.MLKEM512 => 1632,
            MLKEMLevel.MLKEM768 => 2400,
            MLKEMLevel.MLKEM1024 => 3168,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    /// <summary>
    /// Returns the size of a public key in bytes for this security level.
    /// </summary>
    /// <param name="level">The ML-KEM security level.</param>
    /// <returns>The public key size in bytes.</returns>
    public static int PublicKeySize(this MLKEMLevel level)
    {
        return level switch
        {
            MLKEMLevel.MLKEM512 => 800,
            MLKEMLevel.MLKEM768 => 1184,
            MLKEMLevel.MLKEM1024 => 1568,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    /// <summary>
    /// Returns the size of a ciphertext in bytes for this security level.
    /// </summary>
    /// <param name="level">The ML-KEM security level.</param>
    /// <returns>The ciphertext size in bytes.</returns>
    public static int CiphertextSize(this MLKEMLevel level)
    {
        return level switch
        {
            MLKEMLevel.MLKEM512 => 768,
            MLKEMLevel.MLKEM768 => 1088,
            MLKEMLevel.MLKEM1024 => 1568,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    /// <summary>
    /// Converts an integer to the corresponding ML-KEM level.
    /// </summary>
    /// <param name="value">The integer value (512, 768, or 1024).</param>
    /// <returns>The corresponding <see cref="MLKEMLevel"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the value does not correspond to a valid ML-KEM level.
    /// </exception>
    public static MLKEMLevel FromInt(int value)
    {
        return value switch
        {
            512 => MLKEMLevel.MLKEM512,
            768 => MLKEMLevel.MLKEM768,
            1024 => MLKEMLevel.MLKEM1024,
            _ => throw BCComponentsException.PostQuantum($"Invalid MLKEM level: {value}"),
        };
    }
}
