using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Static helper for generating complete keypairs.
/// </summary>
/// <remarks>
/// Provides convenience methods for generating complete signing and encapsulation
/// keypairs using Schnorr + X25519 as the default algorithms.
/// </remarks>
public static class Keypair
{
    /// <summary>
    /// Generates a new keypair using the system's cryptographically secure RNG.
    /// </summary>
    /// <remarks>
    /// Uses Schnorr for signing and X25519 for encapsulation by default.
    /// </remarks>
    /// <returns>A tuple of (<see cref="PrivateKeys"/>, <see cref="PublicKeys"/>).</returns>
    public static (PrivateKeys PrivateKeys, PublicKeys PublicKeys) Generate()
    {
        return GenerateUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates a new keypair using the given random number generator.
    /// </summary>
    /// <remarks>
    /// Uses Schnorr for signing and X25519 for encapsulation by default.
    /// </remarks>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A tuple of (<see cref="PrivateKeys"/>, <see cref="PublicKeys"/>).</returns>
    public static (PrivateKeys PrivateKeys, PublicKeys PublicKeys) GenerateUsing(IRandomNumberGenerator rng)
    {
        return GenerateUsing(rng, SignatureScheme.Schnorr, EncapsulationScheme.X25519);
    }

    /// <summary>
    /// Generates a new keypair using the given RNG and specific schemes.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="signingScheme">The signing scheme to use.</param>
    /// <param name="encapsulationScheme">The encapsulation scheme to use.</param>
    /// <returns>A tuple of (<see cref="PrivateKeys"/>, <see cref="PublicKeys"/>).</returns>
    public static (PrivateKeys PrivateKeys, PublicKeys PublicKeys) GenerateUsing(
        IRandomNumberGenerator rng,
        SignatureScheme signingScheme,
        EncapsulationScheme encapsulationScheme)
    {
        var (sigPriv, sigPub) = signingScheme.KeypairUsing(rng, "");
        var (encPriv, encPub) = encapsulationScheme.KeypairUsing(rng);

        var privateKeys = new PrivateKeys(sigPriv, encPriv);
        var publicKeys = new PublicKeys(sigPub, encPub);
        return (privateKeys, publicKeys);
    }
}
