package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.DigestProvider

/**
 * Inclusion proof extension for Gordian Envelopes.
 *
 * Inclusion proofs allow proving that specific elements exist within an
 * envelope without revealing the entire contents. This leverages the
 * Merkle-like digest tree structure of envelopes.
 */

/** Creates a proof that this envelope includes every element in the target set. */
fun Envelope.proofContainsSet(target: Set<Digest>): Envelope? {
    val revealSet = revealSetOfSet(target)
    if (!target.all { it in revealSet }) return null
    return elideRevealingSet(revealSet).elideRemovingSet(target)
}

/** Creates a proof that this envelope includes the single target element. */
fun Envelope.proofContainsTarget(target: DigestProvider): Envelope? =
    proofContainsSet(setOf(target.digest()))

/** Verifies that all target elements exist using the given proof. */
fun Envelope.confirmContainsSet(target: Set<Digest>, proof: Envelope): Boolean =
    digest() == proof.digest() && proof.containsAll(target)

/** Verifies that the target element exists using the given proof. */
fun Envelope.confirmContainsTarget(target: DigestProvider, proof: Envelope): Boolean =
    confirmContainsSet(setOf(target.digest()), proof)

// -- Internal implementation --

/** Builds a set of all digests needed to reveal the target set. */
private fun Envelope.revealSetOfSet(target: Set<Digest>): Set<Digest> {
    val result = mutableSetOf<Digest>()
    revealSets(target, emptySet(), result)
    return result
}

/** Checks if this envelope contains all elements in the target set. */
private fun Envelope.containsAll(target: Set<Digest>): Boolean {
    val remaining = target.toMutableSet()
    removeAllFound(remaining)
    return remaining.isEmpty()
}

/**
 * Recursively collects all digests forming the path from root to each
 * target element.
 */
private fun Envelope.revealSets(
    target: Set<Digest>,
    current: Set<Digest>,
    result: MutableSet<Digest>,
) {
    val currentWithSelf = current + digest()

    if (digest() in target) {
        result.addAll(currentWithSelf)
    }

    when (val c = case()) {
        is EnvelopeCase.Node -> {
            c.subject.revealSets(target, currentWithSelf, result)
            for (assertion in c.assertions) {
                assertion.revealSets(target, currentWithSelf, result)
            }
        }
        is EnvelopeCase.Wrapped -> {
            c.envelope.revealSets(target, currentWithSelf, result)
        }
        is EnvelopeCase.AssertionCase -> {
            c.assertion.predicate().revealSets(target, currentWithSelf, result)
            c.assertion.objectEnvelope().revealSets(target, currentWithSelf, result)
        }
        else -> {}
    }
}

/**
 * Recursively traverses the envelope and removes found target elements
 * from the set.
 */
private fun Envelope.removeAllFound(target: MutableSet<Digest>) {
    target.remove(digest())
    if (target.isEmpty()) return

    when (val c = case()) {
        is EnvelopeCase.Node -> {
            c.subject.removeAllFound(target)
            for (assertion in c.assertions) {
                assertion.removeAllFound(target)
            }
        }
        is EnvelopeCase.Wrapped -> {
            c.envelope.removeAllFound(target)
        }
        is EnvelopeCase.AssertionCase -> {
            c.assertion.predicate().removeAllFound(target)
            c.assertion.objectEnvelope().removeAllFound(target)
        }
        else -> {}
    }
}
