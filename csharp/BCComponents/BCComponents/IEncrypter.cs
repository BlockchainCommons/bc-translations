namespace BlockchainCommons.BCComponents;

/// <summary>
/// Interface for types that can encapsulate shared secrets for public key encryption.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IEncrypter"/> interface defines the contract for encapsulating
/// a shared secret using a public key. This is a key part of hybrid encryption
/// schemes, where a shared symmetric key is encapsulated with a public key, and
/// the recipient uses their private key to recover the symmetric key.
/// </para>
/// <para>
/// Types implementing this interface provide the ability to:
/// <list type="number">
/// <item>Access their encapsulation public key</item>
/// <item>Generate and encapsulate new shared secrets</item>
/// </list>
/// </para>
/// </remarks>
public interface IEncrypter
{
    /// <summary>
    /// Returns the encapsulation public key for this encrypter.
    /// </summary>
    /// <returns>The <see cref="EncapsulationPublicKey"/> used for encapsulation.</returns>
    EncapsulationPublicKey EncapsulationPublicKey();

    /// <summary>
    /// Encapsulates a new shared secret for the recipient.
    /// </summary>
    /// <remarks>
    /// Generates a new shared secret and encapsulates it using the encapsulation
    /// public key from this encrypter.
    /// </remarks>
    /// <returns>
    /// A tuple containing the generated <see cref="SymmetricKey"/> shared secret
    /// and the <see cref="EncapsulationCiphertext"/> that can be sent to the recipient.
    /// </returns>
    (SymmetricKey SharedKey, EncapsulationCiphertext Ciphertext) EncapsulateNewSharedSecret()
    {
        return EncapsulationPublicKey().EncapsulateNewSharedSecret();
    }
}

/// <summary>
/// Interface for types that can decapsulate shared secrets for public key decryption.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IDecrypter"/> interface defines the contract for decapsulating
/// (recovering) a shared secret using a private key. This is the counterpart to
/// <see cref="IEncrypter"/> and is used by the recipient of encapsulated messages.
/// </para>
/// <para>
/// Types implementing this interface provide the ability to:
/// <list type="number">
/// <item>Access their encapsulation private key</item>
/// <item>Decapsulate shared secrets from ciphertexts</item>
/// </list>
/// </para>
/// </remarks>
public interface IDecrypter
{
    /// <summary>
    /// Returns the encapsulation private key for this decrypter.
    /// </summary>
    /// <returns>The <see cref="EncapsulationPrivateKey"/> used for decapsulation.</returns>
    EncapsulationPrivateKey EncapsulationPrivateKey();

    /// <summary>
    /// Decapsulates a shared secret from a ciphertext.
    /// </summary>
    /// <param name="ciphertext">
    /// The <see cref="EncapsulationCiphertext"/> containing the encapsulated shared secret.
    /// </param>
    /// <returns>The decapsulated <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the ciphertext type does not match the private key type, or if
    /// decapsulation fails.
    /// </exception>
    SymmetricKey DecapsulateSharedSecret(EncapsulationCiphertext ciphertext)
    {
        return EncapsulationPrivateKey().DecapsulateSharedSecret(ciphertext);
    }
}
