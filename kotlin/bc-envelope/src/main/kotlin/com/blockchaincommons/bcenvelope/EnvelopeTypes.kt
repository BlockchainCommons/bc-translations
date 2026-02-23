package com.blockchaincommons.bcenvelope

import com.blockchaincommons.knownvalues.IS_A
import com.blockchaincommons.knownvalues.KnownValue

/**
 * Type system extensions for Gordian Envelopes.
 *
 * Types are represented as assertions with the 'isA' predicate.
 */

/** Adds a type assertion using the 'isA' predicate. */
fun Envelope.addType(objectValue: Any): Envelope = addAssertion(IS_A, objectValue)

/** Returns all type objects from 'isA' assertions. */
fun Envelope.types(): List<Envelope> = objectsForPredicate(IS_A)

/** Returns the single type object, or throws if zero or multiple. */
fun Envelope.getType(): Envelope {
    val t = types()
    if (t.size == 1) return t[0]
    throw EnvelopeException.AmbiguousType()
}

/** Returns true if the envelope has the given type. */
fun Envelope.hasType(t: Any): Boolean {
    val e = Envelope.from(t)
    return types().any { it.digest() == e.digest() }
}

/** Returns true if the envelope has the given KnownValue type. */
fun Envelope.hasTypeValue(t: KnownValue): Boolean {
    val typeEnvelope = t.toEnvelope()
    return types().any { it.digest() == typeEnvelope.digest() }
}

/** Verifies that the envelope has the given KnownValue type. */
fun Envelope.checkTypeValue(t: KnownValue) {
    if (!hasTypeValue(t)) throw EnvelopeException.InvalidType()
}

/** Verifies that the envelope has the given type. */
fun Envelope.checkType(t: Any) {
    if (!hasType(t)) throw EnvelopeException.InvalidType()
}
