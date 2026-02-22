package com.blockchaincommons.bccomponents

/**
 * A type that can encapsulate shared secrets for public key encryption.
 *
 * The [Encrypter] interface defines an interface for encapsulating a shared
 * secret using a public key. This is a key part of hybrid encryption schemes,
 * where a shared symmetric key is encapsulated with a public key, and the
 * recipient uses their private key to recover the symmetric key.
 *
 * Types implementing this interface provide the ability to:
 * 1. Access their encapsulation public key
 * 2. Generate and encapsulate new shared secrets
 */
interface Encrypter {
    /** Returns the encapsulation public key for this encrypter. */
    fun encapsulationPublicKey(): EncapsulationPublicKey

    /**
     * Encapsulates a new shared secret for the recipient.
     *
     * This method generates a new shared secret and encapsulates it using
     * the encapsulation public key from this encrypter.
     *
     * @return a pair containing the generated shared secret as a [SymmetricKey]
     *   and the [EncapsulationCiphertext] that can be sent to the recipient
     */
    fun encapsulateNewSharedSecret(): Pair<SymmetricKey, EncapsulationCiphertext> =
        encapsulationPublicKey().encapsulateNewSharedSecret()
}

/**
 * A type that can decapsulate shared secrets for public key decryption.
 *
 * The [Decrypter] interface defines an interface for decapsulating (recovering)
 * a shared secret using a private key. This is the counterpart to the
 * [Encrypter] interface and is used by the recipient of encapsulated messages.
 *
 * Types implementing this interface provide the ability to:
 * 1. Access their encapsulation private key
 * 2. Decapsulate shared secrets from ciphertexts
 */
interface Decrypter {
    /** Returns the encapsulation private key for this decrypter. */
    fun encapsulationPrivateKey(): EncapsulationPrivateKey

    /**
     * Decapsulates a shared secret from a ciphertext.
     *
     * This method recovers the shared secret that was encapsulated in the
     * given ciphertext, using the private key from this decrypter.
     *
     * @param ciphertext the encapsulation ciphertext containing the
     *   encapsulated shared secret
     * @return the decapsulated [SymmetricKey]
     * @throws BcComponentsException.Crypto if the ciphertext type does not
     *   match the private key type, or if decapsulation fails
     */
    fun decapsulateSharedSecret(ciphertext: EncapsulationCiphertext): SymmetricKey =
        encapsulationPrivateKey().decapsulateSharedSecret(ciphertext)
}
