namespace BlockchainCommons.BCComponents;

/// <summary>
/// A type that can provide a complete set of private cryptographic keys.
/// </summary>
/// <remarks>
/// Types implementing this interface can be used as a source of
/// <see cref="PrivateKeys"/>, which contain both signing and encryption
/// private keys.
/// </remarks>
public interface IPrivateKeysProvider
{
    /// <summary>
    /// Returns a complete set of private keys for cryptographic operations.
    /// </summary>
    PrivateKeys PrivateKeys();
}
