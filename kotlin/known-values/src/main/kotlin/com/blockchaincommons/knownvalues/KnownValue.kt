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
 * Equality and hashing are based only on the numeric codepoint, matching the
 * Rust reference semantics.
 */
class KnownValue private constructor(
    private val rawValue: ULong,
    private val assignedNameValue: String?,
) : DigestProvider, CborTaggedCodable {

    /** Returns the numeric codepoint for this known value. */
    fun value(): ULong = rawValue

    /** Returns the assigned name if present. */
    fun assignedName(): String? = assignedNameValue

    /** Returns the assigned name or the numeric value as text. */
    fun name(): String = assignedNameValue ?: rawValue.toString()

    override fun toString(): String = name()

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is KnownValue) return false
        return rawValue == other.rawValue
    }

    override fun hashCode(): Int = rawValue.hashCode()

    override fun digest(): Digest = Digest.fromImage(taggedCborData())

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_KNOWN_VALUE_VALUE))

    override fun untaggedCbor(): Cbor = Cbor.fromUnsigned(rawValue)

    companion object {
        /** Creates a known value with no assigned name. */
        fun new(value: ULong): KnownValue = KnownValue(value, null)

        /** Creates a known value with no assigned name. */
        fun new(value: Long): KnownValue = KnownValue(value.toULong(), null)

        /** Creates a known value with a dynamic assigned name. */
        fun newWithName(value: ULong, assignedName: String): KnownValue {
            return KnownValue(value, assignedName)
        }

        /** Creates a known value with a dynamic assigned name. */
        fun newWithName(value: Long, assignedName: String): KnownValue {
            return KnownValue(value.toULong(), assignedName)
        }

        /**
         * Creates a known value with a static-style name.
         *
         * Kotlin does not support Rust-style `const fn`, so this is a regular
         * factory used for top-level constant initialization.
         */
        fun newWithStaticName(value: ULong, name: String): KnownValue {
            return KnownValue(value, name)
        }

        /** Decodes a known value from untagged CBOR (an unsigned integer). */
        fun fromUntaggedCbor(cbor: Cbor): KnownValue = new(cbor.tryULong())

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

        /** Creates from Rust-equivalent `From<u64>`. */
        fun fromULong(value: ULong): KnownValue = new(value)

        /** Creates from Rust-equivalent `From<i32>` cast semantics. */
        fun fromInt(value: Int): KnownValue = new(value.toLong())

        /** Creates from Rust-equivalent `From<usize>` cast semantics. */
        fun fromUSize(value: Int): KnownValue = new(value.toULong())
    }
}

private const val TAG_KNOWN_VALUE_VALUE: ULong = 40000uL
