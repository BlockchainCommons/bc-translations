namespace BlockchainCommons.BCComponents;

/// <summary>
/// A type that can provide a complete set of public cryptographic keys.
/// </summary>
/// <remarks>
/// Types implementing this interface can be used as a source of
/// <see cref="PublicKeys"/>, which contain both verification and encryption
/// public keys.
/// </remarks>
public interface IPublicKeysProvider
{
    /// <summary>
    /// Returns a complete set of public keys for cryptographic operations.
    /// </summary>
    PublicKeys PublicKeys();
}
