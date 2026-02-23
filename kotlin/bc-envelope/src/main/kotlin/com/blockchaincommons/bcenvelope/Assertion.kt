package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.DigestProvider
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborMap

/**
 * A predicate-object relationship representing an assertion about a subject.
 *
 * In Gordian Envelope, assertions are the basic building blocks for attaching
 * information to a subject. An assertion consists of a predicate (which states
 * what is being asserted) and an object (which provides the assertion's value).
 *
 * The digest is calculated from the digests of the predicate and object.
 */
class Assertion private constructor(
    private val predicate: Envelope,
    private val objectEnvelope: Envelope,
    private val _digest: Digest,
) : DigestProvider {

    /** Returns the predicate envelope. */
    fun predicate(): Envelope = predicate

    /** Returns the object envelope. */
    fun objectEnvelope(): Envelope = objectEnvelope

    override fun digest(): Digest = _digest

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Assertion) return false
        return _digest == other._digest
    }

    override fun hashCode(): Int = _digest.hashCode()

    override fun toString(): String = "Assertion(${predicate.format()}: ${objectEnvelope.format()})"

    /** Converts this assertion to its CBOR representation (a single-element map). */
    fun toCbor(): Cbor {
        val map = CborMap()
        map.insert(predicate.untaggedCbor(), objectEnvelope.untaggedCbor())
        return Cbor.fromMap(map)
    }

    companion object {
        /**
         * Creates a new assertion from a predicate and object.
         *
         * Both predicate and object are converted to envelopes via [EnvelopeEncodable].
         */
        operator fun invoke(
            predicate: EnvelopeEncodable,
            objectValue: EnvelopeEncodable,
        ): Assertion {
            val predicateEnvelope = predicate.toEnvelope()
            val objectEnvelope = objectValue.toEnvelope()
            val digest = Digest.fromDigests(
                listOf(predicateEnvelope.digest(), objectEnvelope.digest())
            )
            return Assertion(predicateEnvelope, objectEnvelope, digest)
        }

        /**
         * Creates an assertion from a CBOR map with exactly one entry.
         *
         * @throws EnvelopeException.InvalidAssertion if the map does not have exactly one entry.
         */
        fun fromCborMap(map: CborMap): Assertion {
            val entries = map.toList()
            if (entries.size != 1) {
                throw EnvelopeException.InvalidAssertion()
            }
            val (keyCbor, valueCbor) = entries[0]
            val predicate = Envelope.fromUntaggedCbor(keyCbor)
            val objectEnvelope = Envelope.fromUntaggedCbor(valueCbor)
            return invoke(predicate, objectEnvelope)
        }

        /**
         * Creates an assertion from a CBOR value (must be a map).
         *
         * @throws EnvelopeException.InvalidAssertion if the CBOR is not a map.
         */
        fun fromCbor(cbor: Cbor): Assertion {
            if (!cbor.isMap()) {
                throw EnvelopeException.InvalidAssertion()
            }
            return fromCborMap(cbor.tryMap())
        }
    }
}
