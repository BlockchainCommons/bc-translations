package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.knownvalues.EDGE
import com.blockchaincommons.knownvalues.IS_A
import com.blockchaincommons.knownvalues.IS_A_RAW
import com.blockchaincommons.knownvalues.SOURCE
import com.blockchaincommons.knownvalues.SOURCE_RAW
import com.blockchaincommons.knownvalues.TARGET
import com.blockchaincommons.knownvalues.TARGET_RAW

/**
 * Edge support for Gordian Envelopes.
 *
 * Edges represent verifiable claims in an envelope graph structure
 * as defined in BCR-2026-003.
 */

/** Adds an edge envelope assertion. */
fun Envelope.addEdgeEnvelope(edge: Envelope): Envelope =
    addAssertion(EDGE, edge)

/** Returns all edge object envelopes. */
fun Envelope.edges(): List<Envelope> = objectsForPredicate(EDGE)

/** Validates an edge envelope's structure per BCR-2026-003. */
fun Envelope.validateEdge() {
    val inner = if (subject().isWrapped()) subject().unwrap() else this

    var seenIsA = false
    var seenSource = false
    var seenTarget = false

    for (assertion in inner.assertions()) {
        val predicate = try {
            assertion.tryPredicate().tryKnownValue().value
        } catch (_: Exception) {
            throw EnvelopeException.EdgeUnexpectedAssertion()
        }
        when (predicate) {
            IS_A_RAW -> {
                if (seenIsA) throw EnvelopeException.EdgeDuplicateIsA()
                seenIsA = true
            }
            SOURCE_RAW -> {
                if (seenSource) throw EnvelopeException.EdgeDuplicateSource()
                seenSource = true
            }
            TARGET_RAW -> {
                if (seenTarget) throw EnvelopeException.EdgeDuplicateTarget()
                seenTarget = true
            }
            else -> throw EnvelopeException.EdgeUnexpectedAssertion()
        }
    }

    if (!seenIsA) throw EnvelopeException.EdgeMissingIsA()
    if (!seenSource) throw EnvelopeException.EdgeMissingSource()
    if (!seenTarget) throw EnvelopeException.EdgeMissingTarget()
}

/** Extracts the 'isA' assertion object from an edge envelope. */
fun Envelope.edgeIsA(): Envelope {
    val inner = if (subject().isWrapped()) subject().unwrap() else this
    return inner.objectForPredicate(IS_A)
}

/** Extracts the 'source' assertion object from an edge envelope. */
fun Envelope.edgeSource(): Envelope {
    val inner = if (subject().isWrapped()) subject().unwrap() else this
    return inner.objectForPredicate(SOURCE)
}

/** Extracts the 'target' assertion object from an edge envelope. */
fun Envelope.edgeTarget(): Envelope {
    val inner = if (subject().isWrapped()) subject().unwrap() else this
    return inner.objectForPredicate(TARGET)
}

/** Extracts the edge's subject identifier. */
fun Envelope.edgeSubject(): Envelope {
    val inner = if (subject().isWrapped()) subject().unwrap() else this
    return inner.subject()
}

/** Filters edges by optional criteria. */
fun Envelope.edgesMatching(
    isA: Envelope? = null,
    source: Envelope? = null,
    target: Envelope? = null,
    subject: Envelope? = null,
): List<Envelope> {
    val allEdges = edges()
    return allEdges.filter { edge ->
        if (isA != null) {
            val edgeIsA = try { edge.edgeIsA() } catch (_: Exception) { return@filter false }
            if (!edgeIsA.isEquivalentTo(isA)) return@filter false
        }
        if (source != null) {
            val edgeSource = try { edge.edgeSource() } catch (_: Exception) { return@filter false }
            if (!edgeSource.isEquivalentTo(source)) return@filter false
        }
        if (target != null) {
            val edgeTarget = try { edge.edgeTarget() } catch (_: Exception) { return@filter false }
            if (!edgeTarget.isEquivalentTo(target)) return@filter false
        }
        if (subject != null) {
            val edgeSubject = try { edge.edgeSubject() } catch (_: Exception) { return@filter false }
            if (!edgeSubject.isEquivalentTo(subject)) return@filter false
        }
        true
    }
}

/**
 * A container for edge envelopes.
 */
class Edges {
    private val envelopes = mutableMapOf<Digest, Envelope>()

    /** Adds an edge envelope. */
    fun add(edgeEnvelope: Envelope) {
        envelopes[edgeEnvelope.digest()] = edgeEnvelope
    }

    /** Retrieves an edge by digest. */
    fun get(digest: Digest): Envelope? = envelopes[digest]

    /** Removes an edge by digest. */
    fun remove(digest: Digest): Envelope? = envelopes.remove(digest)

    /** Removes all edges. */
    fun clear() = envelopes.clear()

    /** Returns true if there are no edges. */
    fun isEmpty(): Boolean = envelopes.isEmpty()

    /** Returns the number of edges. */
    val size: Int get() = envelopes.size

    /** Returns an iterator over all edge envelopes. */
    val entries: Set<Map.Entry<Digest, Envelope>> get() = envelopes.entries

    /** Adds all edges as assertions to the given envelope. */
    fun addToEnvelope(envelope: Envelope): Envelope {
        var result = envelope
        for ((_, edgeEnvelope) in envelopes) {
            result = result.addAssertion(EDGE, edgeEnvelope)
        }
        return result
    }

    companion object {
        /** Extracts edges from an envelope. */
        fun fromEnvelope(envelope: Envelope): Edges {
            val edges = Edges()
            for (edgeEnv in envelope.edges()) {
                edges.envelopes[edgeEnv.digest()] = edgeEnv
            }
            return edges
        }
    }
}

/**
 * Interface for types that can have edges.
 */
interface Edgeable {
    fun edgesContainer(): Edges
    fun mutableEdgesContainer(): Edges
}
