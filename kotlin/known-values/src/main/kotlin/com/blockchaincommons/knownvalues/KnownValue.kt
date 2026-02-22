package com.blockchaincommons.knownvalues

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.DigestProvider
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A value in a namespace of unsigned integers that represents a stand-alone
 * ontological concept.
 *
 * Equality and hashing are based only on the numeric [value], matching the
 * reference semantics where two known values with different names but the same
 * codepoint are considered equal.
 *
 * @property value The numeric codepoint for this known value.
 * @property assignedName The human-readable name assigned to this value, if any.
 */
class KnownValue private constructor(
    val value: ULong,
    val assignedName: String?,
) : DigestProvider, CborTaggedCodable {

    /** Returns the assigned name, or the numeric codepoint as text if unnamed. */
    val name: String get() = assignedName ?: value.toString()

    override fun toString(): String = name

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is KnownValue) return false
        return value == other.value
    }

    override fun hashCode(): Int = value.hashCode()

    override fun digest(): Digest = Digest.fromImage(taggedCborData())

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_KNOWN_VALUE_VALUE))

    override fun untaggedCbor(): Cbor = Cbor.fromUnsigned(value)

    companion object {
        /** Creates a known value with no assigned name. */
        operator fun invoke(value: ULong): KnownValue = KnownValue(value, null)

        /** Creates a known value with no assigned name. */
        operator fun invoke(value: Long): KnownValue = KnownValue(value.toULong(), null)

        /** Creates a known value with no assigned name. */
        operator fun invoke(value: Int): KnownValue = KnownValue(value.toULong(), null)

        /** Creates a known value with an assigned name. */
        fun withName(value: ULong, assignedName: String): KnownValue {
            return KnownValue(value, assignedName)
        }

        /** Creates a known value with an assigned name. */
        fun withName(value: Long, assignedName: String): KnownValue {
            return KnownValue(value.toULong(), assignedName)
        }

        /** Decodes a known value from untagged CBOR (an unsigned integer). */
        fun fromUntaggedCbor(cbor: Cbor): KnownValue = KnownValue(cbor.tryULong())

        /** Decodes a known value from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): KnownValue =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_KNOWN_VALUE_VALUE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a known value from tagged CBOR data bytes. */
        fun fromTaggedCborData(data: ByteArray): KnownValue =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_KNOWN_VALUE_VALUE)),
            ) { fromUntaggedCbor(it) }
    }
}

private const val TAG_KNOWN_VALUE_VALUE: ULong = 40000uL
