package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.ED25519_PUBLIC_KEY_SIZE
import com.blockchaincommons.bccrypto.ED25519_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.ed25519Verify

/**
 * An Ed25519 public key for verifying digital signatures.
 *
 * Ed25519 public keys are 32 bytes and are used to verify signatures created
 * with the corresponding [Ed25519PrivateKey]. The Ed25519 signature system
 * provides fast verification, small keys, and high security.
 */
class Ed25519PublicKey private constructor(private val data: ByteArray) :
    ReferenceProvider {

    init {
        require(data.size == KEY_SIZE) {
            "Ed25519 public key must be exactly $KEY_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying key data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the key bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /**
     * Verifies an Ed25519 signature for the given message.
     *
     * @param signature a 64-byte Ed25519 signature
     * @param message the message that was signed
     * @return `true` if the signature is valid, `false` otherwise
     */
    fun verify(signature: ByteArray, message: ByteArray): Boolean {
        require(signature.size == ED25519_SIGNATURE_SIZE) {
            "Ed25519 signature must be exactly $ED25519_SIGNATURE_SIZE bytes, got ${signature.size}"
        }
        return ed25519Verify(data, message, signature)
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(data))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Ed25519PublicKey) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "Ed25519PublicKey(${refHexShort()})"

    companion object {
        const val KEY_SIZE: Int = ED25519_PUBLIC_KEY_SIZE

        /** Restores an Ed25519 public key from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): Ed25519PublicKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "Ed25519 public key", KEY_SIZE, data.size,
                )
            }
            return Ed25519PublicKey(data.copyOf())
        }

        /**
         * Creates an Ed25519 public key from a hexadecimal string.
         *
         * @throws BcComponentsException.InvalidSize if the decoded bytes
         *   are not exactly [KEY_SIZE] bytes
         */
        fun fromHex(hex: String): Ed25519PublicKey =
            fromData(hex.hexToByteArray())
    }
}
