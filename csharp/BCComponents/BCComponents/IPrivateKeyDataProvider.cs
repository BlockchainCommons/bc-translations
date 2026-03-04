namespace BlockchainCommons.BCComponents;

/// <summary>
/// A type that can provide unique data for cryptographic key derivation.
/// </summary>
/// <remarks>
/// Types implementing <see cref="IPrivateKeyDataProvider"/> can be used as seed
/// material for cryptographic key derivation. The provided data should be
/// sufficiently random and unpredictable to ensure the security of the derived
/// keys.
/// </remarks>
public interface IPrivateKeyDataProvider
{
    /// <summary>
    /// Returns unique data from which cryptographic keys can be derived.
    /// </summary>
    /// <returns>A byte array containing the private key data.</returns>
    byte[] PrivateKeyData();
}
