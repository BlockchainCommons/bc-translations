package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.DigestProvider
import com.blockchaincommons.knownvalues.ATTACHMENT
import com.blockchaincommons.knownvalues.CONFORMS_TO
import com.blockchaincommons.knownvalues.VENDOR

/**
 * Attachment support for Gordian Envelopes.
 *
 * Attachments allow vendor-specific data to be included in an envelope
 * without interfering with the main data structure.
 */

// -- Assertion-level attachment methods --

/** Creates a new attachment assertion. */
fun Assertion.Companion.newAttachment(
    payload: Any,
    vendor: String,
    conformsTo: String? = null,
): Assertion {
    val payloadEnvelope = payload.asEnvelopeEncodable().toEnvelope()
        .wrap()
        .addAssertion(VENDOR, vendor)
    val withConformsTo = if (conformsTo != null) {
        payloadEnvelope.addAssertion(CONFORMS_TO, conformsTo)
    } else {
        payloadEnvelope
    }
    return Assertion(ATTACHMENT.asEnvelopeEncodable(), withConformsTo.asEnvelopeEncodable())
}

// -- Envelope attachment methods --

/** Creates a new attachment envelope. */
fun Envelope.Companion.newAttachment(
    payload: Any,
    vendor: String,
    conformsTo: String? = null,
): Envelope = Assertion.newAttachment(payload, vendor, conformsTo).toEnvelope()

/** Adds an attachment to this envelope. */
fun Envelope.addAttachment(
    payload: Any,
    vendor: String,
    conformsTo: String? = null,
): Envelope = addAssertionEnvelope(
    Assertion.newAttachment(payload, vendor, conformsTo).toEnvelope(),
)

/** Returns the payload of an attachment envelope. */
fun Envelope.attachmentPayload(): Envelope {
    val c = case()
    if (c !is EnvelopeCase.AssertionCase) throw EnvelopeException.InvalidAttachment()
    return c.assertion.objectEnvelope().unwrap()
}

/** Returns the vendor of an attachment envelope. */
fun Envelope.attachmentVendor(): String {
    val c = case()
    if (c !is EnvelopeCase.AssertionCase) throw EnvelopeException.InvalidAttachment()
    return c.assertion.objectEnvelope().extractObjectForPredicate(VENDOR)
}

/** Returns the optional conformsTo of an attachment envelope. */
fun Envelope.attachmentConformsTo(): String? {
    val c = case()
    if (c !is EnvelopeCase.AssertionCase) throw EnvelopeException.InvalidAttachment()
    return c.assertion.objectEnvelope().extractOptionalObjectForPredicate(CONFORMS_TO)
}

/** Returns all attachments in the envelope. */
fun Envelope.attachments(): List<Envelope> =
    attachmentsWithVendorAndConformsTo(null, null)

/** Returns attachments matching the given vendor and/or conformsTo. */
fun Envelope.attachmentsWithVendorAndConformsTo(
    vendor: String?,
    conformsTo: String?,
): List<Envelope> {
    val assertions = assertionsWithPredicate(ATTACHMENT)
    return assertions.filter { assertion ->
        if (vendor != null) {
            val v = try { assertion.attachmentVendor() } catch (_: Exception) { return@filter false }
            if (v != vendor) return@filter false
        }
        if (conformsTo != null) {
            val c = try { assertion.attachmentConformsTo() } catch (_: Exception) { return@filter false }
            if (c != conformsTo) return@filter false
        }
        true
    }
}

/** Returns the single attachment matching the criteria, or throws. */
fun Envelope.attachmentWithVendorAndConformsTo(
    vendor: String?,
    conformsTo: String?,
): Envelope {
    val attachments = attachmentsWithVendorAndConformsTo(vendor, conformsTo)
    return when {
        attachments.isEmpty() -> throw EnvelopeException.NonexistentAttachment()
        attachments.size > 1 -> throw EnvelopeException.AmbiguousAttachment()
        else -> attachments[0]
    }
}

/** Validates this envelope is a proper attachment envelope. */
fun Envelope.validateAttachment() {
    val c = case()
    if (c !is EnvelopeCase.AssertionCase) throw EnvelopeException.InvalidAttachment()
    attachmentPayload()
    attachmentVendor()
    attachmentConformsTo()
}

/**
 * A container for vendor-specific metadata attachments.
 */
class Attachments {
    private val envelopes = mutableMapOf<Digest, Envelope>()

    /** Adds a new attachment. */
    fun add(payload: Any, vendor: String, conformsTo: String? = null) {
        val attachment = Envelope.newAttachment(payload, vendor, conformsTo)
        envelopes[attachment.digest()] = attachment
    }

    /** Retrieves an attachment by digest. */
    fun get(digest: Digest): Envelope? = envelopes[digest]

    /** Removes an attachment by digest. */
    fun remove(digest: Digest): Envelope? = envelopes.remove(digest)

    /** Removes all attachments. */
    fun clear() = envelopes.clear()

    /** Returns true if there are no attachments. */
    fun isEmpty(): Boolean = envelopes.isEmpty()

    /** Adds all attachments as assertions to the given envelope. */
    fun addToEnvelope(envelope: Envelope): Envelope {
        var result = envelope
        for ((_, att) in envelopes) {
            result = result.addAssertionEnvelope(att)
        }
        return result
    }

    companion object {
        /** Extracts attachments from an envelope. */
        fun fromEnvelope(envelope: Envelope): Attachments {
            val attachments = Attachments()
            for (att in envelope.attachments()) {
                attachments.envelopes[att.digest()] = att
            }
            return attachments
        }
    }
}

/**
 * Interface for types that can have metadata attachments.
 */
interface Attachable {
    fun attachments(): Attachments
    fun mutableAttachments(): Attachments
}
