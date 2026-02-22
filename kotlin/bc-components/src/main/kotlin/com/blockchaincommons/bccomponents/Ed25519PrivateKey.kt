package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.ED25519_PRIVATE_KEY_SIZE
import com.blockchaincommons.bccrypto.ED25519_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.deriveSigningPrivateKey
import com.blockchaincommons.bccrypto.ed25519PublicKeyFromPrivateKey
import com.blockchaincommons.bccrypto.ed25519Sign
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator

/**
 * An Ed25519 private key for creating digital signatures.
 *
 * Ed25519 is a public-key signature system based on the Edwards curve over
 * the finite field GF(2^255 - 19). It provides fast signature generation,
 * high security (128-bit equivalent symmetric security), collision resilience,
 * side-channel protection, and compact keys (32 bytes) and signatures (64 bytes).
 */
class Ed25519PrivateKey private constructor(private val data: ByteArray) :
    ReferenceProvider {

    init {
        require(data.size == KEY_SIZE) {
            "Ed25519 private key must be exactly $KEY_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying key data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the key bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Derives the corresponding Ed25519 public key. */
    fun publicKey(): Ed25519PublicKey =
        Ed25519PublicKey.fromData(ed25519PublicKeyFromPrivateKey(data))

    /**
     * Signs a message using this Ed25519 private key.
     *
     * @param message the data to sign
     * @return a 64-byte Ed25519 signature
     */
    fun sign(message: ByteArray): ByteArray {
        val sig = ed25519Sign(data, message)
        require(sig.size == ED25519_SIGNATURE_SIZE)
        return sig
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(data))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Ed25519PrivateKey) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "Ed25519PrivateKey(${refHexShort()})"

    companion object {
        const val KEY_SIZE: Int = ED25519_PRIVATE_KEY_SIZE

        /** Creates a new random Ed25519 private key using the secure RNG. */
        fun create(): Ed25519PrivateKey {
            val rng = SecureRandomNumberGenerator()
            return createUsing(rng)
        }

        /** Creates a new random Ed25519 private key using the given [rng]. */
        fun createUsing(rng: RandomNumberGenerator): Ed25519PrivateKey {
            val keyData = rng.randomData(KEY_SIZE)
            return Ed25519PrivateKey(keyData)
        }

        /** Restores an Ed25519 private key from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): Ed25519PrivateKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "Ed25519 private key", KEY_SIZE, data.size,
                )
            }
            return Ed25519PrivateKey(data.copyOf())
        }

        /**
         * Derives an Ed25519 private key from arbitrary key material.
         *
         * Uses HKDF to deterministically derive a valid key from the
         * given material.
         */
        fun deriveFromKeyMaterial(keyMaterial: ByteArray): Ed25519PrivateKey =
            fromData(deriveSigningPrivateKey(keyMaterial))

        /**
         * Creates an Ed25519 private key from a hexadecimal string.
         *
         * @throws BcComponentsException.InvalidSize if the decoded bytes
         *   are not exactly [KEY_SIZE] bytes
         */
        fun fromHex(hex: String): Ed25519PrivateKey =
            fromData(hex.hexToByteArray())
    }
}
