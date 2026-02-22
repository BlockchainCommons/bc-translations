package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.aeadChaCha20Poly1305Decrypt
import com.blockchaincommons.bccrypto.aeadChaCha20Poly1305Encrypt
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bctags.TAG_SYMMETRIC_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A symmetric encryption key used for both encryption and decryption.
 *
 * [SymmetricKey] is a 32-byte cryptographic key used with ChaCha20-Poly1305
 * AEAD (Authenticated Encryption with Associated Data) encryption. This
 * implementation follows the IETF ChaCha20-Poly1305 specification as defined
 * in [RFC-8439](https://datatracker.ietf.org/doc/html/rfc8439).
 *
 * Symmetric encryption uses the same key for both encryption and decryption,
 * unlike asymmetric encryption where different keys are used for each
 * operation.
 */
class SymmetricKey private constructor(private val data: ByteArray) :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == SYMMETRIC_KEY_SIZE) {
            "SymmetricKey data must be exactly $SYMMETRIC_KEY_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 32-byte key data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the key bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the key bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    /**
     * Encrypts the given plaintext using this key.
     *
     * @param plaintext the data to encrypt
     * @param aad optional additional authenticated data
     * @param nonce optional nonce; if `null`, a new random nonce is generated
     * @return the [EncryptedMessage] containing ciphertext, nonce, auth tag,
     *   and AAD
     */
    fun encrypt(
        plaintext: ByteArray,
        aad: ByteArray? = null,
        nonce: Nonce? = null,
    ): EncryptedMessage {
        val effectiveAad = aad ?: ByteArray(0)
        val effectiveNonce = nonce ?: Nonce.create()
        val (ciphertext, auth) = aeadChaCha20Poly1305Encrypt(
            plaintext,
            data,
            effectiveNonce.data(),
            effectiveAad,
        )
        return EncryptedMessage(
            ciphertext,
            effectiveAad,
            effectiveNonce,
            AuthenticationTag.fromData(auth),
        )
    }

    /**
     * Encrypts the given plaintext using this key and includes the digest
     * of the plaintext in the AAD field.
     *
     * @param plaintext the data to encrypt
     * @param digest the digest of the plaintext
     * @param nonce optional nonce; if `null`, a new random nonce is generated
     * @return the [EncryptedMessage] containing ciphertext, nonce, auth tag,
     *   and the CBOR-encoded digest as AAD
     */
    fun encryptWithDigest(
        plaintext: ByteArray,
        digest: Digest,
        nonce: Nonce? = null,
    ): EncryptedMessage {
        val digestCborData = digest.taggedCbor().toCborData()
        return encrypt(plaintext, digestCborData, nonce)
    }

    /**
     * Decrypts the given [EncryptedMessage] using this key.
     *
     * @param message the encrypted message to decrypt
     * @return the decrypted plaintext
     * @throws BcComponentsException.Crypto if decryption fails
     */
    fun decrypt(message: EncryptedMessage): ByteArray {
        return try {
            aeadChaCha20Poly1305Decrypt(
                message.ciphertext(),
                data,
                message.nonce().data(),
                message.authenticationTag().data(),
                message.aad(),
            )
        } catch (e: Exception) {
            throw BcComponentsException.crypto("decryption failed: ${e.message}")
        }
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is SymmetricKey) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "SymmetricKey(${refHexShort()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_SYMMETRIC_KEY))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        const val SYMMETRIC_KEY_SIZE: Int = 32

        /** Creates a new random symmetric key using the secure RNG. */
        fun create(): SymmetricKey {
            val rng = SecureRandomNumberGenerator()
            return createUsing(rng)
        }

        /** Creates a new random symmetric key using the given [rng]. */
        fun createUsing(rng: RandomNumberGenerator): SymmetricKey {
            val data = rng.randomData(SYMMETRIC_KEY_SIZE)
            return SymmetricKey(data)
        }

        /** Restores a [SymmetricKey] from exactly [SYMMETRIC_KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): SymmetricKey {
            require(data.size == SYMMETRIC_KEY_SIZE) {
                "SymmetricKey data must be exactly $SYMMETRIC_KEY_SIZE bytes, got ${data.size}"
            }
            return SymmetricKey(data.copyOf())
        }

        /**
         * Restores a [SymmetricKey] from a byte array, throwing a
         * [BcComponentsException] if the length is wrong.
         */
        fun fromDataChecked(data: ByteArray): SymmetricKey {
            if (data.size != SYMMETRIC_KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "symmetric key",
                    SYMMETRIC_KEY_SIZE,
                    data.size,
                )
            }
            return SymmetricKey(data.copyOf())
        }

        /**
         * Creates a [SymmetricKey] from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64
         *   hex digits.
         */
        fun fromHex(hex: String): SymmetricKey = fromData(hex.hexToByteArray())

        /** Decodes a [SymmetricKey] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): SymmetricKey {
            val bytes = cbor.tryByteStringData()
            return fromDataChecked(bytes)
        }

        /** Decodes a [SymmetricKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): SymmetricKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SYMMETRIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SymmetricKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): SymmetricKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SYMMETRIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SymmetricKey] from a UR. */
        fun fromUr(ur: UR): SymmetricKey {
            ur.checkType("crypto-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [SymmetricKey] from a UR string. */
        fun fromUrString(urString: String): SymmetricKey =
            fromUr(UR.fromUrString(urString))
    }
}
