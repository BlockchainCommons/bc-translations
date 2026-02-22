package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_SEALED_MESSAGE
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A sealed message that can only be decrypted by the intended recipient.
 *
 * [SealedMessage] provides a public key encryption mechanism where a message is
 * encrypted with a symmetric key, and that key is then encapsulated using the
 * recipient's public key. This ensures that only the recipient can decrypt the
 * message by first decapsulating the shared secret using their private key.
 *
 * Features:
 * - Anonymous sender: The sender's identity is not revealed in the sealed
 *   message
 * - Authenticated encryption: Message integrity and authenticity are guaranteed
 * - Forward secrecy: Each message uses a different ephemeral key
 * - Post-quantum security options: Can use ML-KEM for quantum-resistant
 *   encryption
 *
 * The structure internally contains:
 * - An [EncryptedMessage] containing the actual encrypted data
 * - An [EncapsulationCiphertext] containing the encapsulated shared secret
 *
 * CDDL:
 * ```
 * SealedMessage = #6.40019([ encrypted_message, encapsulated_key ])
 * ```
 */
class SealedMessage private constructor(
    private val message: EncryptedMessage,
    private val encapsulatedKey: EncapsulationCiphertext,
) : CborTaggedCodable,
    URCodable {

    /** Returns the encrypted message. */
    fun message(): EncryptedMessage = message

    /** Returns the encapsulated key ciphertext. */
    fun encapsulatedKey(): EncapsulationCiphertext = encapsulatedKey

    /**
     * Returns the encapsulation scheme used for this sealed message.
     */
    fun encapsulationScheme(): EncapsulationScheme =
        encapsulatedKey.encapsulationScheme()

    /**
     * Decrypts the message using the recipient's private key.
     *
     * This method performs the following steps:
     * 1. Decapsulates the shared secret using the recipient's private key
     * 2. Uses the shared secret to decrypt the message
     *
     * @param privateKey the private key of the intended recipient
     * @return the decrypted plaintext
     * @throws BcComponentsException.Crypto if decryption fails
     */
    fun decrypt(privateKey: Decrypter): ByteArray {
        val sharedKey = privateKey.decapsulateSharedSecret(encapsulatedKey)
        return sharedKey.decrypt(message)
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is SealedMessage) return false
        return message == other.message && encapsulatedKey == other.encapsulatedKey
    }

    override fun hashCode(): Int {
        var result = message.hashCode()
        result = 31 * result + encapsulatedKey.hashCode()
        return result
    }

    // -- toString --

    override fun toString(): String =
        "SealedMessage(scheme=${encapsulationScheme()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_SEALED_MESSAGE))

    override fun untaggedCbor(): Cbor {
        val messageCbor = message.taggedCbor()
        val encapsulatedKeyCbor = encapsulatedKey.taggedCbor()
        return Cbor.fromArray(listOf(messageCbor, encapsulatedKeyCbor))
    }

    // -- Companion --

    companion object {
        /**
         * Creates a new [SealedMessage] by encrypting [plaintext] for the
         * specified [recipient].
         *
         * @param plaintext the message data to encrypt
         * @param recipient the recipient who will be able to decrypt the message
         * @param aad additional authenticated data (optional, not encrypted but
         *   authenticated)
         * @param testNonce optional nonce for deterministic encryption (testing only)
         * @return a new [SealedMessage]
         */
        fun create(
            plaintext: ByteArray,
            recipient: Encrypter,
            aad: ByteArray? = null,
            testNonce: Nonce? = null,
        ): SealedMessage {
            val (sharedKey, encapsulatedKey) = recipient.encapsulateNewSharedSecret()
            val message = sharedKey.encrypt(plaintext, aad, testNonce)
            return SealedMessage(message, encapsulatedKey)
        }

        /** Decodes a [SealedMessage] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): SealedMessage {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "SealedMessage",
                    "must have exactly 2 elements, got ${elements.size}",
                )
            }
            val message = EncryptedMessage.fromTaggedCbor(elements[0])
            val encapsulatedKey = EncapsulationCiphertext.fromTaggedCbor(elements[1])
            return SealedMessage(message, encapsulatedKey)
        }

        /** Decodes a [SealedMessage] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): SealedMessage =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SEALED_MESSAGE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SealedMessage] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): SealedMessage =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SEALED_MESSAGE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SealedMessage] from a UR. */
        fun fromUr(ur: UR): SealedMessage {
            ur.checkType("crypto-sealed")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [SealedMessage] from a UR string. */
        fun fromUrString(urString: String): SealedMessage =
            fromUr(UR.fromUrString(urString))
    }
}
