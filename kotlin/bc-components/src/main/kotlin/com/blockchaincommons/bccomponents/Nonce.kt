package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.fillRandomData
import com.blockchaincommons.bctags.TAG_NONCE
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A random nonce ("number used once").
 *
 * A [Nonce] is a 12-byte random value used in cryptographic protocols to
 * prevent replay attacks and ensure the uniqueness of encrypted messages.
 */
class Nonce private constructor(private val data: ByteArray) :
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == NONCE_SIZE) {
            "Nonce data must be exactly $NONCE_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying nonce data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the nonce bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The nonce as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the nonce bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Nonce) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "Nonce($hex)"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_NONCE))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        const val NONCE_SIZE: Int = 12

        /** Creates a new random nonce using the secure random number generator. */
        fun create(): Nonce {
            val data = ByteArray(NONCE_SIZE)
            fillRandomData(data)
            return Nonce(data)
        }

        /** Restores a nonce from exactly [NONCE_SIZE] bytes. */
        fun fromData(data: ByteArray): Nonce {
            require(data.size == NONCE_SIZE) {
                "Nonce data must be exactly $NONCE_SIZE bytes, got ${data.size}"
            }
            return Nonce(data.copyOf())
        }

        /**
         * Restores a nonce from a byte array, throwing if the length is wrong.
         */
        fun fromDataChecked(data: ByteArray): Nonce {
            if (data.size != NONCE_SIZE) {
                throw BcComponentsException.invalidSize("nonce", NONCE_SIZE, data.size)
            }
            return Nonce(data.copyOf())
        }

        /**
         * Creates a nonce from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 24
         *   hex digits.
         */
        fun fromHex(hex: String): Nonce = fromData(hex.hexToByteArray())

        /** Decodes a [Nonce] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): Nonce {
            val bytes = cbor.tryByteStringData()
            return fromDataChecked(bytes)
        }

        /** Decodes a [Nonce] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Nonce =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_NONCE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Nonce] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Nonce =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_NONCE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Nonce] from a UR. */
        fun fromUr(ur: UR): Nonce {
            ur.checkType("nonce")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Nonce] from a UR string. */
        fun fromUrString(urString: String): Nonce =
            fromUr(UR.fromUrString(urString))
    }
}
