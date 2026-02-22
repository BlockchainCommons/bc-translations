package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_X25519_PUBLIC_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A public key for X25519 key agreement operations.
 *
 * X25519 is an elliptic-curve Diffie-Hellman key exchange protocol based on
 * Curve25519 as defined in [RFC 7748](https://datatracker.ietf.org/doc/html/rfc7748).
 * It allows two parties to establish a shared secret key over an insecure
 * channel.
 *
 * The X25519 public key is generated from a corresponding private key and is
 * designed to be:
 * - Compact (32 bytes)
 * - Fast to use in key agreement operations
 * - Resistant to various cryptographic attacks
 */
class X25519PublicKey private constructor(private val data: ByteArray) :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == KEY_SIZE) {
            "X25519PublicKey data must be exactly $KEY_SIZE bytes, got ${data.size}"
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

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is X25519PublicKey) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "X25519PublicKey(${refHexShort()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_X25519_PUBLIC_KEY))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        const val KEY_SIZE: Int = 32

        /** Restores an [X25519PublicKey] from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): X25519PublicKey {
            require(data.size == KEY_SIZE) {
                "X25519PublicKey data must be exactly $KEY_SIZE bytes, got ${data.size}"
            }
            return X25519PublicKey(data.copyOf())
        }

        /**
         * Restores an [X25519PublicKey] from a byte array, throwing a
         * [BcComponentsException] if the length is wrong.
         */
        fun fromDataChecked(data: ByteArray): X25519PublicKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "X25519 public key",
                    KEY_SIZE,
                    data.size,
                )
            }
            return X25519PublicKey(data.copyOf())
        }

        /**
         * Creates an [X25519PublicKey] from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64
         *   hex digits.
         */
        fun fromHex(hex: String): X25519PublicKey = fromData(hex.hexToByteArray())

        /** Decodes an [X25519PublicKey] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): X25519PublicKey {
            val bytes = cbor.tryByteStringData()
            return fromDataChecked(bytes)
        }

        /** Decodes an [X25519PublicKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): X25519PublicKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_X25519_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [X25519PublicKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): X25519PublicKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_X25519_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [X25519PublicKey] from a UR. */
        fun fromUr(ur: UR): X25519PublicKey {
            ur.checkType("agreement-public-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [X25519PublicKey] from a UR string. */
        fun fromUrString(urString: String): X25519PublicKey =
            fromUr(UR.fromUrString(urString))
    }
}
