using BlockchainCommons.BCComponents;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Secret-based encryption extension for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Provides methods for locking envelopes with password or other
/// secret-derived keys, and for unlocking them. Uses the <see cref="EncryptedKey"/>
/// mechanism from BCComponents to encrypt the content key using a key derivation
/// method and a secret.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Locks the envelope's subject with a secret using the given key derivation method.
    /// </summary>
    /// <remarks>
    /// Generates a random content key, encrypts the subject with it, then encrypts
    /// the content key using the specified derivation method and secret. The encrypted
    /// key is added as a <c>hasSecret</c> assertion.
    /// </remarks>
    /// <param name="method">The key derivation method to use.</param>
    /// <param name="secret">The secret bytes to derive the encryption key from.</param>
    /// <returns>A new envelope with encrypted subject and <c>hasSecret</c> assertion.</returns>
    public Envelope LockSubject(KeyDerivationMethod method, byte[] secret)
    {
        var contentKey = SymmetricKey.New();
        var encryptedKey = EncryptedKey.Lock(method, secret, contentKey);
        return EncryptSubject(contentKey)
            .AddAssertion(KnownValuesRegistry.HasSecret, encryptedKey);
    }

    /// <summary>
    /// Unlocks the envelope's subject with a secret.
    /// </summary>
    /// <remarks>
    /// Tries each <c>hasSecret</c> assertion to find one that can be unlocked
    /// with the given secret, then uses the recovered content key to decrypt
    /// the envelope's subject.
    /// </remarks>
    /// <param name="secret">The secret bytes to derive the decryption key from.</param>
    /// <returns>A new envelope with decrypted subject.</returns>
    /// <exception cref="EnvelopeException">Thrown if no secret assertion can be unlocked.</exception>
    public Envelope UnlockSubject(byte[] secret)
    {
        foreach (var assertion in AssertionsWithPredicate(KnownValuesRegistry.HasSecret))
        {
            var obj = assertion.AsObject()!;
            if (obj.IsObscured)
                continue;
            try
            {
                var encryptedKey = obj.ExtractSubject<EncryptedKey>();
                var contentKey = encryptedKey.Unlock(secret);
                return DecryptSubject(contentKey);
            }
            catch
            {
                // Try next assertion
            }
        }
        throw EnvelopeException.UnknownSecret();
    }

    /// <summary>
    /// Returns <c>true</c> if the envelope has a password-based <c>hasSecret</c> assertion.
    /// </summary>
    public bool IsLockedWithPassword()
    {
        return AssertionsWithPredicate(KnownValuesRegistry.HasSecret)
            .Any(assertion =>
            {
                try
                {
                    var obj = assertion.AsObject()!;
                    var encryptedKey = obj.ExtractSubject<EncryptedKey>();
                    return encryptedKey.IsPasswordBased;
                }
                catch
                {
                    return false;
                }
            });
    }

    /// <summary>
    /// Returns <c>true</c> if the envelope has an SSH agent-based <c>hasSecret</c> assertion.
    /// </summary>
    public bool IsLockedWithSshAgent()
    {
        return AssertionsWithPredicate(KnownValuesRegistry.HasSecret)
            .Any(assertion =>
            {
                try
                {
                    var obj = assertion.AsObject()!;
                    var encryptedKey = obj.ExtractSubject<EncryptedKey>();
                    return encryptedKey.IsSshAgent;
                }
                catch
                {
                    return false;
                }
            });
    }

    /// <summary>
    /// Adds a <c>hasSecret</c> assertion to an already-encrypted envelope.
    /// </summary>
    /// <param name="method">The key derivation method to use.</param>
    /// <param name="secret">The secret bytes to derive the encryption key from.</param>
    /// <param name="contentKey">The content key that was used to encrypt the subject.</param>
    /// <returns>A new envelope with the <c>hasSecret</c> assertion added.</returns>
    public Envelope AddSecret(KeyDerivationMethod method, byte[] secret, SymmetricKey contentKey)
    {
        var encryptedKey = EncryptedKey.Lock(method, secret, contentKey);
        return AddAssertion(KnownValuesRegistry.HasSecret, encryptedKey);
    }

    /// <summary>
    /// Wraps and locks the envelope's subject with a secret.
    /// </summary>
    /// <param name="method">The key derivation method to use.</param>
    /// <param name="secret">The secret bytes to derive the encryption key from.</param>
    /// <returns>A new wrapped and locked envelope.</returns>
    public Envelope Lock(KeyDerivationMethod method, byte[] secret)
    {
        return Wrap().LockSubject(method, secret);
    }

    /// <summary>
    /// Unlocks the envelope's subject and unwraps it.
    /// </summary>
    /// <param name="secret">The secret bytes to derive the decryption key from.</param>
    /// <returns>The original, unwrapped envelope.</returns>
    public Envelope Unlock(byte[] secret)
    {
        return UnlockSubject(secret).TryUnwrap();
    }
}
