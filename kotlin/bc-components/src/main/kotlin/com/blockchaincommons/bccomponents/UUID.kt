package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.fillRandomData
import com.blockchaincommons.bctags.TAG_UUID
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A Universally Unique Identifier (UUID).
 *
 * UUIDs are 128-bit (16-byte) identifiers designed to be unique across space
 * and time. This implementation creates type 4 (random) UUIDs following the
 * UUID specification:
 *
 * - Version field (bits 48-51) is set to 4, indicating a random UUID
 * - Variant field (bits 64-65) is set to 2, indicating RFC 4122 variant
 *
 * The canonical textual representation uses 5 groups separated by hyphens:
 * `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
 *
 * Note: This is the bc-components UUID type, not [java.util.UUID].
 */
class UUID private constructor(private val data: ByteArray) :
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == UUID_SIZE) {
            "UUID data must be exactly $UUID_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 16-byte UUID data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the UUID bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /**
     * Returns the canonical UUID string representation.
     *
     * Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
     */
    override fun toString(): String {
        val hex = data.toHexString()
        return "${hex.substring(0, 8)}-${hex.substring(8, 12)}-${hex.substring(12, 16)}-${hex.substring(16, 20)}-${hex.substring(20, 32)}"
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is UUID) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_UUID))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    companion object {
        const val UUID_SIZE: Int = 16

        /** Creates a new random type 4 UUID. */
        fun create(): UUID {
            val bytes = ByteArray(UUID_SIZE)
            fillRandomData(bytes)
            // Set version to 4 (random UUID)
            bytes[6] = ((bytes[6].toInt() and 0x0F) or 0x40).toByte()
            // Set variant to 2 (RFC 4122)
            bytes[8] = ((bytes[8].toInt() and 0x3F) or 0x80).toByte()
            return UUID(bytes)
        }

        /** Restores a UUID from exactly [UUID_SIZE] bytes. */
        fun fromData(data: ByteArray): UUID {
            if (data.size != UUID_SIZE) {
                throw BcComponentsException.invalidSize("UUID", UUID_SIZE, data.size)
            }
            return UUID(data.copyOf())
        }

        /**
         * Parses a UUID from the canonical string representation.
         *
         * Accepts the standard format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
         *
         * @throws IllegalArgumentException if the string is not a valid UUID
         */
        fun fromString(uuidString: String): UUID {
            val stripped = uuidString.trim().replace("-", "")
            val bytes = stripped.hexToByteArray()
            return fromData(bytes)
        }

        /**
         * Creates a UUID from a hexadecimal string (no dashes).
         *
         * @throws IllegalArgumentException if the string is not exactly 32
         *   hex digits.
         */
        fun fromHex(hex: String): UUID = fromData(hex.hexToByteArray())

        /** Decodes a [UUID] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): UUID {
            val bytes = cbor.tryByteStringData()
            return fromData(bytes)
        }

        /** Decodes a [UUID] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): UUID =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_UUID)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [UUID] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): UUID =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_UUID)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [UUID] from a UR. */
        fun fromUr(ur: UR): UUID {
            ur.checkType("uuid")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [UUID] from a UR string. */
        fun fromUrString(urString: String): UUID =
            fromUr(UR.fromUrString(urString))
    }
}
