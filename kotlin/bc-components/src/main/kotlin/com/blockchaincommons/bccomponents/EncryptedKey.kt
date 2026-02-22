package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_ENCRYPTED_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A symmetric content key encrypted using secret-based key derivation.
 *
 * [EncryptedKey] wraps an [EncryptedMessage] whose ciphertext is the
 * encrypted content key and whose AAD contains the CBOR-encoded key
 * derivation parameters. Multiple derivation methods are supported
 * (HKDF, PBKDF2, Scrypt, Argon2id).
 *
 * CDDL:
 * ```
 * EncryptedKey = #6.40027(EncryptedMessage)
 * ```
 *
 * Usage:
 * - Call [lock] to encrypt a content key with a chosen derivation method.
 * - Call [unlock] to recover the content key from a previously locked
 *   encrypted key.
 */
class EncryptedKey private constructor(
    private val params: KeyDerivationParams,
    private val encryptedMessage: EncryptedMessage,
) : CborTaggedCodable,
    URCodable {

    /** Returns the underlying encrypted message. */
    fun encryptedMessage(): EncryptedMessage = encryptedMessage

    /** Returns `true` if the derivation method is password-based. */
    fun isPasswordBased(): Boolean = params.isPasswordBased()

    /**
     * Decrypts this encrypted key using the given [secret] to recover the
     * original content key.
     *
     * The derivation parameters are read from the AAD of the encrypted
     * message, and the same key derivation is applied to produce the
     * decryption key.
     *
     * @throws BcComponentsException if decryption fails (e.g. wrong secret).
     */
    fun unlock(secret: ByteArray): SymmetricKey {
        val aad = encryptedMessage.aad()
        if (aad.isEmpty()) {
            throw BcComponentsException.general("Missing AAD CBOR in EncryptedMessage")
        }
        val paramsCbor = Cbor.tryFromData(aad)
        val a = paramsCbor.tryArray()
        if (a.isEmpty()) {
            throw BcComponentsException.general("Empty KeyDerivation array in AAD")
        }
        val index = a[0].tryInt()
        val method = KeyDerivationMethod.fromIndex(index)
            ?: throw BcComponentsException.general("Invalid KeyDerivationMethod index: $index")
        return when (method) {
            KeyDerivationMethod.HKDF ->
                HKDFParams.fromCbor(paramsCbor).unlock(encryptedMessage, secret)
            KeyDerivationMethod.PBKDF2 ->
                PBKDF2Params.fromCbor(paramsCbor).unlock(encryptedMessage, secret)
            KeyDerivationMethod.Scrypt ->
                ScryptParams.fromCbor(paramsCbor).unlock(encryptedMessage, secret)
            KeyDerivationMethod.Argon2id ->
                Argon2idParams.fromCbor(paramsCbor).unlock(encryptedMessage, secret)
        }
    }

    // -- toString --

    override fun toString(): String = "EncryptedKey($params)"

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is EncryptedKey) return false
        return params == other.params && encryptedMessage == other.encryptedMessage
    }

    override fun hashCode(): Int = 31 * params.hashCode() + encryptedMessage.hashCode()

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_ENCRYPTED_KEY))

    /**
     * The untagged CBOR representation is the tagged CBOR of the inner
     * [EncryptedMessage]. This means the wire format is:
     * `#6.40027(#6.40002([ciphertext, nonce, auth, aad]))`.
     */
    override fun untaggedCbor(): Cbor = encryptedMessage.taggedCbor()

    // -- Companion --

    companion object {
        /**
         * Encrypts a [contentKey] using the given key derivation [method]
         * and [secret].
         *
         * Default parameters are used for the chosen method.
         */
        fun lock(
            method: KeyDerivationMethod,
            secret: ByteArray,
            contentKey: SymmetricKey,
        ): EncryptedKey {
            val params = when (method) {
                KeyDerivationMethod.HKDF ->
                    KeyDerivationParams.HKDF(HKDFParams())
                KeyDerivationMethod.PBKDF2 ->
                    KeyDerivationParams.PBKDF2(PBKDF2Params())
                KeyDerivationMethod.Scrypt ->
                    KeyDerivationParams.Scrypt(ScryptParams())
                KeyDerivationMethod.Argon2id ->
                    KeyDerivationParams.Argon2id(Argon2idParams())
            }
            return lock(params, secret, contentKey)
        }

        /**
         * Encrypts a [contentKey] using the given key derivation [params]
         * and [secret].
         *
         * This overload allows specifying custom derivation parameters.
         */
        fun lock(
            params: KeyDerivationParams,
            secret: ByteArray,
            contentKey: SymmetricKey,
        ): EncryptedKey {
            val encryptedMessage = params.lock(contentKey, secret)
            return EncryptedKey(params, encryptedMessage)
        }

        /** Decodes an [EncryptedKey] from untagged CBOR. */
        fun fromUntaggedCbor(cbor: Cbor): EncryptedKey {
            val encryptedMessage = EncryptedMessage.fromTaggedCbor(cbor)
            val aad = encryptedMessage.aad()
            if (aad.isEmpty()) {
                throw BcComponentsException.general("Missing AAD in EncryptedKey")
            }
            val paramsCbor = Cbor.tryFromData(aad)
            val params = KeyDerivationParams.fromCbor(paramsCbor)
            return EncryptedKey(params, encryptedMessage)
        }

        /** Decodes an [EncryptedKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): EncryptedKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_ENCRYPTED_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [EncryptedKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): EncryptedKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_ENCRYPTED_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [EncryptedKey] from a UR. */
        fun fromUr(ur: UR): EncryptedKey {
            ur.checkType("encrypted-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [EncryptedKey] from a UR string. */
        fun fromUrString(urString: String): EncryptedKey =
            fromUr(UR.fromUrString(urString))
    }
}
