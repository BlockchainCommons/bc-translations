package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bctags.TAG_FUNCTION
import com.blockchaincommons.dcbor.*

/**
 * A function identifier used in Gordian Envelope expressions.
 *
 * Functions can be identified by a numeric ID (well-known) or a string name
 * (application-specific). Encoded in CBOR with tag #6.40006.
 */
sealed class Function {
    /** A well-known function identified by a numeric ID with an optional name. */
    class Known(val value: ULong, val name: String? = null) : Function()

    /** A function identified by a string name. */
    class Named(val name: String) : Function()

    /** Returns the display name of this function. */
    fun name(): String = when (this) {
        is Known -> name ?: value.toString()
        is Named -> "\"$name\""
    }

    /** Returns the raw name for named functions, or null for known functions. */
    fun namedName(): String? = when (this) {
        is Known -> null
        is Named -> name
    }

    // -- CBOR --

    fun cborTags(): List<Tag> = Companion.cborTags()

    fun taggedCbor(): Cbor = Cbor(CborCase.Tagged(cborTags().first(), untaggedCbor()))

    fun untaggedCbor(): Cbor = when (this) {
        is Known -> Cbor.fromUnsigned(value)
        is Named -> Cbor.fromString(name)
    }

    // -- EnvelopeEncodable --

    fun toEnvelope(): Envelope = Envelope.newLeaf(taggedCbor())

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Function) return false
        return when {
            this is Known && other is Known -> value == other.value
            this is Named && other is Named -> name == other.name
            else -> false
        }
    }

    override fun hashCode(): Int = when (this) {
        is Known -> value.hashCode()
        is Named -> name.hashCode()
    }

    override fun toString(): String = FunctionsStore.nameForFunction(this, null)

    companion object {
        fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_FUNCTION))

        fun fromUntaggedCbor(cbor: Cbor): Function {
            return when {
                cbor.cborCase is CborCase.Unsigned -> Known(cbor.tryULong(), null)
                cbor.cborCase is CborCase.Text -> Named(cbor.tryText())
                else -> throw CborException.Custom("invalid function")
            }
        }

        fun fromTaggedCbor(cbor: Cbor): Function =
            CborTaggedUtils.fromTaggedCbor(cbor, cborTags()) { fromUntaggedCbor(it) }
    }
}
