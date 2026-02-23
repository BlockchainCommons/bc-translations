package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bctags.TAG_PARAMETER
import com.blockchaincommons.dcbor.*

/**
 * A parameter identifier used in Gordian Envelope expressions.
 *
 * In Gordian Envelope, a parameter appears as a predicate in an assertion on
 * an expression envelope. The parameter identifies the name of the argument,
 * and the object of the assertion is the argument value.
 *
 * Parameters can be identified by a numeric ID (well-known) or a string name
 * (application-specific). Encoded in CBOR with tag #6.40007.
 */
sealed class Parameter {
    /** A well-known parameter identified by a numeric ID with an optional name. */
    class Known(val value: ULong, val name: String? = null) : Parameter()

    /** A parameter identified by a string name. */
    class Named(val name: String) : Parameter()

    /** Returns the display name of this parameter. */
    fun name(): String = when (this) {
        is Known -> name ?: value.toString()
        is Named -> "\"$name\""
    }

    /** Returns the raw name for named parameters, or null for known parameters. */
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
        if (other !is Parameter) return false
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

    override fun toString(): String = ParametersStore.nameForParameter(this, null)

    companion object {
        fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_PARAMETER))

        fun fromUntaggedCbor(cbor: Cbor): Parameter {
            return when {
                cbor.cborCase is CborCase.Unsigned -> Known(cbor.tryULong(), null)
                cbor.cborCase is CborCase.Text -> Named(cbor.tryText())
                else -> throw CborException.Custom("invalid parameter")
            }
        }

        fun fromTaggedCbor(cbor: Cbor): Parameter =
            CborTaggedUtils.fromTaggedCbor(cbor, cborTags()) { fromUntaggedCbor(it) }
    }
}
