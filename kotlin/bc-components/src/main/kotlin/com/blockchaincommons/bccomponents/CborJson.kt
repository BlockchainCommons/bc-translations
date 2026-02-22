package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_JSON
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A CBOR-tagged container for UTF-8 JSON text.
 *
 * [CborJson] wraps UTF-8 JSON text as a CBOR byte string with tag 262.
 * This allows JSON data to be embedded within CBOR structures while
 * maintaining type information through the tag.
 *
 * Note: this type does not validate that the contained data is well-formed
 * JSON; it simply provides a type-safe wrapper.
 *
 * Named `CborJson` (rather than `JSON`) to avoid collisions with Kotlin's
 * JSON libraries.
 */
class CborJson private constructor(private val data: ByteArray) :
    CborTaggedCodable,
    URCodable {

    /** The length of the JSON data in bytes. */
    val size: Int get() = data.size

    /** Whether the JSON data is empty. */
    val isEmpty: Boolean get() = data.isEmpty()

    /** Returns the data as a byte copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /**
     * Returns the data as a UTF-8 string.
     *
     * @throws IllegalStateException if the data is not valid UTF-8.
     */
    fun asString(): String = data.toString(Charsets.UTF_8)

    /** The data as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the JSON bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is CborJson) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "JSON(${asString()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_JSON))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        /** Creates a [CborJson] instance from raw byte data. */
        fun fromData(data: ByteArray): CborJson = CborJson(data.copyOf())

        /** Creates a [CborJson] instance from a UTF-8 string. */
        fun fromString(s: String): CborJson = CborJson(s.toByteArray(Charsets.UTF_8))

        /**
         * Creates a [CborJson] instance from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the hex string is invalid.
         */
        fun fromHex(hex: String): CborJson = fromData(hex.hexToByteArray())

        /** Decodes a [CborJson] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): CborJson {
            val bytes = cbor.tryByteStringData()
            return CborJson(bytes)
        }

        /** Decodes a [CborJson] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): CborJson =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_JSON)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [CborJson] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): CborJson =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_JSON)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [CborJson] from a UR. */
        fun fromUr(ur: UR): CborJson {
            ur.checkType("json")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [CborJson] from a UR string. */
        fun fromUrString(urString: String): CborJson =
            fromUr(UR.fromUrString(urString))
    }
}
