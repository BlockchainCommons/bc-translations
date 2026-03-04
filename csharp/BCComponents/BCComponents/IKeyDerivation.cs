namespace BlockchainCommons.BCComponents;

/// <summary>
/// Interface for key derivation implementations.
/// </summary>
/// <remarks>
/// Each implementation derives a symmetric key from a secret and uses that
/// derived key to lock (encrypt) or unlock (decrypt) a content key. The
/// derivation parameters are serialised as CBOR and stored in the encrypted
/// message's Additional Authenticated Data (AAD) so that unlock can recover
/// them.
/// </remarks>
public interface IKeyDerivation
{
    /// <summary>
    /// Derives a key from <paramref name="secret"/>, then encrypts
    /// <paramref name="contentKey"/> with it.
    /// </summary>
    /// <param name="contentKey">The content key to encrypt.</param>
    /// <param name="secret">The secret to derive the encryption key from.</param>
    /// <returns>An <see cref="EncryptedMessage"/> with derivation params in the AAD.</returns>
    EncryptedMessage Lock(SymmetricKey contentKey, byte[] secret);

    /// <summary>
    /// Derives a key from <paramref name="secret"/>, then decrypts
    /// <paramref name="encryptedMessage"/> to recover the original content key.
    /// </summary>
    /// <param name="encryptedMessage">The encrypted message to decrypt.</param>
    /// <param name="secret">The secret to derive the decryption key from.</param>
    /// <returns>The recovered <see cref="SymmetricKey"/>.</returns>
    SymmetricKey Unlock(EncryptedMessage encryptedMessage, byte[] secret);
}
