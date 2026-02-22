package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.deriveAgreementPrivateKey
import com.blockchaincommons.bccrypto.x25519NewPrivateKeyUsing
import com.blockchaincommons.bccrypto.x25519PublicKeyFromPrivateKey
import com.blockchaincommons.bccrypto.x25519SharedKey
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bctags.TAG_X25519_PRIVATE_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A private key for X25519 key agreement operations.
 *
 * X25519 is an elliptic-curve Diffie-Hellman key exchange protocol based on
 * Curve25519 as defined in [RFC 7748](https://datatracker.ietf.org/doc/html/rfc7748).
 * It allows two parties to establish a shared secret key over an insecure
 * channel.
 *
 * Key features of X25519:
 * - High security (128-bit security level)
 * - High performance
 * - Small key sizes (32 bytes)
 * - Protection against various side-channel attacks
 */
class X25519PrivateKey private constructor(private val data: ByteArray) :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == KEY_SIZE) {
            "X25519PrivateKey data must be exactly $KEY_SIZE bytes, got ${data.size}"
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
     * Derives the [X25519PublicKey] corresponding to this private key.
     */
    fun publicKey(): X25519PublicKey =
        X25519PublicKey.fromData(x25519PublicKeyFromPrivateKey(data))

    /**
     * Derives a shared [SymmetricKey] from this private key and the given
     * [publicKey].
     *
     * Both parties perform this operation with their own private key and
     * the other party's public key, arriving at the same shared key.
     */
    fun sharedKeyWith(publicKey: X25519PublicKey): SymmetricKey =
        SymmetricKey.fromData(x25519SharedKey(data, publicKey.data()))

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is X25519PrivateKey) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "X25519PrivateKey(${refHexShort()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_X25519_PRIVATE_KEY))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        const val KEY_SIZE: Int = 32

        /** Generates a new random [X25519PrivateKey] using the secure RNG. */
        fun create(): X25519PrivateKey {
            val rng = SecureRandomNumberGenerator()
            return createUsing(rng)
        }

        /** Generates a new random [X25519PrivateKey] using the given [rng]. */
        fun createUsing(rng: RandomNumberGenerator): X25519PrivateKey =
            X25519PrivateKey(x25519NewPrivateKeyUsing(rng))

        /**
         * Generates a new random [X25519PrivateKey] and corresponding
         * [X25519PublicKey].
         */
        fun keypair(): Pair<X25519PrivateKey, X25519PublicKey> {
            val privateKey = create()
            return privateKey to privateKey.publicKey()
        }

        /**
         * Generates a new random [X25519PrivateKey] and corresponding
         * [X25519PublicKey] using the given [rng].
         */
        fun keypairUsing(rng: RandomNumberGenerator): Pair<X25519PrivateKey, X25519PublicKey> {
            val privateKey = createUsing(rng)
            return privateKey to privateKey.publicKey()
        }

        /** Restores an [X25519PrivateKey] from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): X25519PrivateKey {
            require(data.size == KEY_SIZE) {
                "X25519PrivateKey data must be exactly $KEY_SIZE bytes, got ${data.size}"
            }
            return X25519PrivateKey(data.copyOf())
        }

        /**
         * Restores an [X25519PrivateKey] from a byte array, throwing a
         * [BcComponentsException] if the length is wrong.
         */
        fun fromDataChecked(data: ByteArray): X25519PrivateKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "X25519 private key",
                    KEY_SIZE,
                    data.size,
                )
            }
            return X25519PrivateKey(data.copyOf())
        }

        /**
         * Derives an [X25519PrivateKey] from the given key material.
         *
         * Uses HKDF to deterministically derive a private key from arbitrary
         * key material.
         */
        fun deriveFromKeyMaterial(keyMaterial: ByteArray): X25519PrivateKey =
            X25519PrivateKey(deriveAgreementPrivateKey(keyMaterial))

        /**
         * Creates an [X25519PrivateKey] from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64
         *   hex digits.
         */
        fun fromHex(hex: String): X25519PrivateKey = fromData(hex.hexToByteArray())

        /** Decodes an [X25519PrivateKey] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): X25519PrivateKey {
            val bytes = cbor.tryByteStringData()
            return fromDataChecked(bytes)
        }

        /** Decodes an [X25519PrivateKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): X25519PrivateKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_X25519_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [X25519PrivateKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): X25519PrivateKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_X25519_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [X25519PrivateKey] from a UR. */
        fun fromUr(ur: UR): X25519PrivateKey {
            ur.checkType("agreement-private-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [X25519PrivateKey] from a UR string. */
        fun fromUrString(urString: String): X25519PrivateKey =
            fromUr(UR.fromUrString(urString))
    }
}
