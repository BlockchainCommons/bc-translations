using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Supported digital signature schemes.
/// </summary>
/// <remarks>
/// This enum represents the various signature schemes supported, including
/// elliptic curve schemes (ECDSA, Schnorr), Edwards curve schemes (Ed25519),
/// post-quantum schemes (ML-DSA), and SSH-specific algorithms.
/// </remarks>
public enum SignatureScheme
{
    /// <summary>BIP-340 Schnorr signature scheme, used in Bitcoin Taproot (default).</summary>
    Schnorr,

    /// <summary>ECDSA signature scheme using the secp256k1 curve.</summary>
    Ecdsa,

    /// <summary>Ed25519 signature scheme (RFC 8032).</summary>
    Ed25519,

    /// <summary>ML-DSA44 post-quantum signature scheme (NIST level 2).</summary>
    MLDSA44,

    /// <summary>ML-DSA65 post-quantum signature scheme (NIST level 3).</summary>
    MLDSA65,

    /// <summary>ML-DSA87 post-quantum signature scheme (NIST level 5).</summary>
    MLDSA87,

    /// <summary>Ed25519 signature scheme for SSH.</summary>
    SshEd25519,

    /// <summary>DSA signature scheme for SSH.</summary>
    SshDsa,

    /// <summary>ECDSA signature scheme with NIST P-256 curve for SSH.</summary>
    SshEcdsaP256,

    /// <summary>ECDSA signature scheme with NIST P-384 curve for SSH.</summary>
    SshEcdsaP384,
}

/// <summary>
/// Extension methods for <see cref="SignatureScheme"/>.
/// </summary>
public static class SignatureSchemeExtensions
{
    /// <summary>
    /// Creates a new key pair for the signature scheme using the system's
    /// secure random number generator.
    /// </summary>
    /// <param name="scheme">The signature scheme to generate keys for.</param>
    /// <returns>A tuple containing a signing private key and its corresponding public key.</returns>
    public static (SigningPrivateKey PrivateKey, SigningPublicKey PublicKey) Keypair(
        this SignatureScheme scheme)
    {
        return scheme.KeypairOpt("");
    }

    /// <summary>
    /// Creates a new key pair for the signature scheme with an optional comment.
    /// </summary>
    /// <remarks>
    /// The comment is only used for SSH keys and is ignored for other schemes.
    /// </remarks>
    /// <param name="scheme">The signature scheme to generate keys for.</param>
    /// <param name="comment">A string comment to include with SSH keys.</param>
    /// <returns>A tuple containing a signing private key and its corresponding public key.</returns>
    public static (SigningPrivateKey PrivateKey, SigningPublicKey PublicKey) KeypairOpt(
        this SignatureScheme scheme,
        string comment)
    {
        switch (scheme)
        {
            case SignatureScheme.Schnorr:
            {
                var privateKey = SigningPrivateKey.NewSchnorr(ECPrivateKey.New());
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.Ecdsa:
            {
                var privateKey = SigningPrivateKey.NewEcdsa(ECPrivateKey.New());
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.Ed25519:
            {
                var privateKey = SigningPrivateKey.NewEd25519(Ed25519PrivateKey.New());
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.MLDSA44:
            {
                var (privKey, pubKey) = MLDSALevel.MLDSA44.Keypair();
                return (SigningPrivateKey.NewMldsa(privKey), SigningPublicKey.FromMldsa(pubKey));
            }
            case SignatureScheme.MLDSA65:
            {
                var (privKey, pubKey) = MLDSALevel.MLDSA65.Keypair();
                return (SigningPrivateKey.NewMldsa(privKey), SigningPublicKey.FromMldsa(pubKey));
            }
            case SignatureScheme.MLDSA87:
            {
                var (privKey, pubKey) = MLDSALevel.MLDSA87.Keypair();
                return (SigningPrivateKey.NewMldsa(privKey), SigningPublicKey.FromMldsa(pubKey));
            }
            case SignatureScheme.SshEd25519:
            {
                var privateKey = SshKeyHelper.GenerateSshSigningPrivateKey(SshKeyHelper.SshAlgorithm.Ed25519, comment);
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.SshDsa:
            {
                var privateKey = SshKeyHelper.GenerateSshSigningPrivateKey(SshKeyHelper.SshAlgorithm.Dsa, comment);
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.SshEcdsaP256:
            {
                var privateKey = SshKeyHelper.GenerateSshSigningPrivateKey(SshKeyHelper.SshAlgorithm.EcdsaP256, comment);
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.SshEcdsaP384:
            {
                var privateKey = SshKeyHelper.GenerateSshSigningPrivateKey(SshKeyHelper.SshAlgorithm.EcdsaP384, comment);
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(scheme), scheme, "Unsupported signature scheme");
        }
    }

    /// <summary>
    /// Creates a key pair for the signature scheme using a provided random number generator.
    /// </summary>
    /// <remarks>
    /// This allows for deterministic key generation when using a seeded RNG.
    /// Note that not all signature schemes support deterministic generation (e.g., ML-DSA does not).
    /// </remarks>
    /// <param name="scheme">The signature scheme to generate keys for.</param>
    /// <param name="rng">A random number generator to use.</param>
    /// <param name="comment">A string comment to include with SSH keys.</param>
    /// <returns>A tuple containing a signing private key and its corresponding public key.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the scheme does not support deterministic generation.
    /// </exception>
    public static (SigningPrivateKey PrivateKey, SigningPublicKey PublicKey) KeypairUsing(
        this SignatureScheme scheme,
        IRandomNumberGenerator rng,
        string comment)
    {
        switch (scheme)
        {
            case SignatureScheme.Schnorr:
            {
                var privateKey = SigningPrivateKey.NewSchnorr(ECPrivateKey.NewUsing(rng));
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.Ecdsa:
            {
                var privateKey = SigningPrivateKey.NewEcdsa(ECPrivateKey.NewUsing(rng));
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            case SignatureScheme.Ed25519:
            {
                var privateKey = SigningPrivateKey.NewEd25519(Ed25519PrivateKey.NewUsing(rng));
                var publicKey = privateKey.PublicKey();
                return (privateKey, publicKey);
            }
            default:
                throw BCComponentsException.General(
                    "Deterministic keypair generation not supported for this signature scheme");
        }
    }
}
