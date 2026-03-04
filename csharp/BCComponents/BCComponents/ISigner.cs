using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Options for configuring signature creation.
/// </summary>
/// <remarks>
/// Different signature schemes may require specific options:
/// <list type="bullet">
/// <item><see cref="SchnorrOptions"/>: Requires a random number generator for
/// nonce generation during Schnorr signature creation.</item>
/// <item><see cref="SshOptions"/>: Requires a namespace and hash algorithm for
/// SSH signature creation.</item>
/// </list>
/// Other signature types like ECDSA, Ed25519, and ML-DSA do not require options.
/// </remarks>
public abstract class SigningOptions
{
    private SigningOptions() { }

    /// <summary>
    /// Options for Schnorr signature creation.
    /// </summary>
    /// <remarks>
    /// Schnorr signatures use a random nonce during creation. By default, the
    /// system's secure RNG is used, but a custom RNG can be provided for
    /// deterministic testing.
    /// </remarks>
    public sealed class SchnorrOptions : SigningOptions
    {
        /// <summary>
        /// The random number generator to use for nonce generation.
        /// </summary>
        public IRandomNumberGenerator Rng { get; }

        /// <summary>
        /// Creates new Schnorr signing options with the specified RNG.
        /// </summary>
        /// <param name="rng">The random number generator to use.</param>
        public SchnorrOptions(IRandomNumberGenerator rng) { Rng = rng; }
    }

    /// <summary>
    /// Options for SSH signature creation.
    /// </summary>
    /// <remarks>
    /// SSH signatures require a namespace string and a hash algorithm.
    /// The namespace identifies the application domain (e.g., "ssh" for SSH
    /// authentication, "file" for file signing).
    /// </remarks>
    public sealed class SshOptions : SigningOptions
    {
        /// <summary>The namespace used for SSH signatures.</summary>
        public string Namespace { get; }

        /// <summary>
        /// The hash algorithm used for SSH signatures (e.g., "sha256" or "sha512").
        /// </summary>
        public string HashAlgorithm { get; }

        /// <summary>
        /// Creates new SSH signing options.
        /// </summary>
        /// <param name="ns">The namespace string.</param>
        /// <param name="hashAlgorithm">The hash algorithm name.</param>
        public SshOptions(string ns, string hashAlgorithm)
        {
            Namespace = ns;
            HashAlgorithm = hashAlgorithm;
        }
    }
}

/// <summary>
/// A type capable of creating digital signatures.
/// </summary>
/// <remarks>
/// The <see cref="ISigner"/> interface provides methods for signing messages with
/// various cryptographic signature schemes. Implementations of this interface can
/// sign messages using different algorithms according to the specific signer type.
/// This interface is implemented by <see cref="SigningPrivateKey"/> for all
/// supported signature schemes.
/// </remarks>
public interface ISigner
{
    /// <summary>
    /// Signs a message with additional options specific to the signature scheme.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <param name="options">Optional signing options (algorithm-specific parameters).</param>
    /// <returns>The digital signature.</returns>
    /// <exception cref="BCComponentsException">Thrown if signing fails.</exception>
    Signature SignWithOptions(byte[] message, SigningOptions? options = null);

    /// <summary>
    /// Signs a message using default options.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>The digital signature.</returns>
    /// <exception cref="BCComponentsException">Thrown if signing fails.</exception>
    Signature Sign(byte[] message) => SignWithOptions(message, null);
}

/// <summary>
/// A type capable of verifying digital signatures.
/// </summary>
/// <remarks>
/// The <see cref="IVerifier"/> interface provides a method to verify that a
/// signature was created by a corresponding signer for a specific message.
/// This interface is implemented by <see cref="SigningPublicKey"/> for all
/// supported signature schemes.
/// </remarks>
public interface IVerifier
{
    /// <summary>
    /// Verifies a signature against a message.
    /// </summary>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="message">The message that was allegedly signed.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid for the message; <c>false</c> otherwise.
    /// </returns>
    bool Verify(Signature signature, byte[] message);
}
