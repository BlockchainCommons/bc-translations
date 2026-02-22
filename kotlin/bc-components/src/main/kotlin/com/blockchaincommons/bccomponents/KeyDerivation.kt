package com.blockchaincommons.bccomponents

/**
 * Interface for key derivation implementations.
 *
 * Each implementation derives a symmetric key from a secret and uses that
 * derived key to lock (encrypt) or unlock (decrypt) a content key. The
 * derivation parameters are serialised as CBOR and stored in the encrypted
 * message's Additional Authenticated Data (AAD) so that unlock can recover
 * them.
 */
interface KeyDerivation {
    /**
     * Derives a key from [secret], then encrypts [contentKey] with it.
     *
     * The CBOR-encoded derivation parameters are placed in the AAD of the
     * returned [EncryptedMessage].
     */
    fun lock(contentKey: SymmetricKey, secret: ByteArray): EncryptedMessage

    /**
     * Derives a key from [secret], then decrypts [encryptedMessage] to
     * recover the original content key.
     */
    fun unlock(encryptedMessage: EncryptedMessage, secret: ByteArray): SymmetricKey
}
