package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.knownvalues.SIGNED

/**
 * Signature support for Gordian Envelopes.
 *
 * Provides methods for digitally signing envelopes and verifying signatures.
 */

/**
 * Metadata associated with a signature in a Gordian Envelope.
 *
 * Provides a way to attach additional information to signatures, such as
 * the signer's identity, the signing date, or the purpose of the signature.
 * When used with the signature extension, this metadata is included in a
 * structured way that is also signed, ensuring the metadata cannot be
 * tampered with without invalidating the signature.
 */
class SignatureMetadata {
    private val assertions = mutableListOf<Assertion>()

    /** Returns the assertions contained in this metadata. */
    fun assertions(): List<Assertion> = assertions.toList()

    /** Returns whether this metadata contains any assertions. */
    fun hasAssertions(): Boolean = assertions.isNotEmpty()

    /** Adds an assertion to this metadata. Returns this for chaining. */
    fun addAssertion(assertion: Assertion): SignatureMetadata {
        assertions.add(assertion)
        return this
    }

    /** Adds a new assertion with the provided predicate and object. Returns this for chaining. */
    fun withAssertion(predicate: Any, obj: Any): SignatureMetadata {
        val predicateEncodable = predicate.asEnvelopeEncodable()
        val objectEncodable = obj.asEnvelopeEncodable()
        assertions.add(Assertion(predicateEncodable, objectEncodable))
        return this
    }
}

/** Adds a signature from the given signer. */
fun Envelope.addSignature(signer: Signer): Envelope =
    addSignatureOpt(signer, null, null)

/** Adds a signature with optional signing options and metadata. */
fun Envelope.addSignatureOpt(
    signer: Signer,
    options: SigningOptions? = null,
    metadata: SignatureMetadata? = null,
): Envelope {
    val digest = subject().digest().data()
    var signature = Envelope.from(
        signer.signWithOptions(digest, options)
    )

    if (metadata != null && metadata.hasAssertions()) {
        var signatureWithMetadata = signature

        metadata.assertions().forEach { assertion ->
            signatureWithMetadata = signatureWithMetadata
                .addAssertionEnvelope(assertion.toEnvelope())
        }

        signatureWithMetadata = signatureWithMetadata.wrap()

        val outerSignature = Envelope.from(
            signer.signWithOptions(
                signatureWithMetadata.digest().data(),
                options,
            )
        )
        signature = signatureWithMetadata
            .addAssertion(SIGNED, outerSignature)
    }

    return addAssertion(SIGNED, signature)
}

/** Adds multiple signatures. */
fun Envelope.addSignatures(signers: List<Signer>): Envelope =
    signers.fold(this) { envelope, signer -> envelope.addSignature(signer) }

// ---- Internal signature checking ----

/** Checks whether the given signature is valid for this envelope's subject. */
private fun Envelope.isSignatureFromKey(
    signature: Signature,
    key: Verifier,
): Boolean = key.verify(signature, subject().digest().data())

/**
 * Checks for a valid signature from the given key, returning the signature
 * metadata envelope if found.
 */
private fun Envelope.hasSomeSignatureFromKeyReturningMetadata(
    key: Verifier,
): Envelope? {
    val signatureObjects = objectsForPredicate(SIGNED)
    for (signatureObject in signatureObjects) {
        val signatureObjectSubject = signatureObject.subject()
        if (signatureObjectSubject.isWrapped()) {
            // Wrapped signature with metadata
            val outerSignatureObject = try {
                signatureObject.objectForPredicate(SIGNED)
            } catch (_: Exception) { continue }

            val outerSignature = try {
                outerSignatureObject.extractSubject<Signature>()
            } catch (_: Exception) {
                throw EnvelopeException.InvalidFormat()
            }

            if (!signatureObjectSubject.isSignatureFromKey(outerSignature, key)) {
                continue
            }

            val signatureMetadataEnvelope = signatureObjectSubject.unwrap()
            val signature = try {
                signatureMetadataEnvelope.extractSubject<Signature>()
            } catch (_: Exception) {
                throw EnvelopeException.InvalidFormat()
            }

            if (!isSignatureFromKey(signature, key)) {
                throw EnvelopeException.UnverifiedSignature()
            }
            return signatureMetadataEnvelope
        } else {
            // Simple signature (not wrapped)
            val signature = try {
                signatureObject.extractSubject<Signature>()
            } catch (_: Exception) {
                throw EnvelopeException.InvalidFormat()
            }

            if (isSignatureFromKey(signature, key)) {
                return signatureObject
            }
        }
    }
    return null
}

/** Returns true if the envelope has a valid signature from the given verifier. */
fun Envelope.hasSignatureFrom(verifier: Verifier): Boolean =
    hasSomeSignatureFromKeyReturningMetadata(verifier) != null

/** Verifies the envelope has a valid signature from the verifier. Throws on failure. */
fun Envelope.verifySignature(verifier: Verifier) {
    if (!hasSignatureFrom(verifier)) throw EnvelopeException.UnverifiedSignature()
}

/** Verifies signature from verifier, returning self on success. */
fun Envelope.verifySignatureFrom(verifier: Verifier): Envelope {
    if (!hasSignatureFrom(verifier)) throw EnvelopeException.UnverifiedSignature()
    return this
}

/** Verifies signature and returns the metadata envelope. */
fun Envelope.verifySignatureFromReturningMetadata(verifier: Verifier): Envelope {
    return hasSomeSignatureFromKeyReturningMetadata(verifier)
        ?: throw EnvelopeException.UnverifiedSignature()
}

/** Checks whether the envelope has a threshold of valid signatures. */
fun Envelope.hasSignaturesFromThreshold(
    verifiers: List<Verifier>,
    threshold: Int? = null,
): Boolean {
    val required = threshold ?: verifiers.size
    var count = 0
    for (key in verifiers) {
        if (hasSomeSignatureFromKeyReturningMetadata(key) != null) {
            count++
            if (count >= required) return true
        }
    }
    return false
}

/** Checks whether the envelope has all required signatures. */
fun Envelope.hasSignaturesFrom(verifiers: List<Verifier>): Boolean =
    hasSignaturesFromThreshold(verifiers)

/** Verifies a threshold of signatures, returning self on success. */
fun Envelope.verifySignaturesFromThreshold(
    verifiers: List<Verifier>,
    threshold: Int? = null,
): Envelope {
    if (!hasSignaturesFromThreshold(verifiers, threshold)) {
        throw EnvelopeException.UnverifiedSignature()
    }
    return this
}

/** Verifies all signatures, returning self on success. */
fun Envelope.verifySignaturesFrom(verifiers: List<Verifier>): Envelope =
    verifySignaturesFromThreshold(verifiers)

// ---- Convenience methods ----

/** Convenience: wrap + add signature. */
fun Envelope.sign(signer: Signer): Envelope =
    signOpt(signer, null)

/** Convenience: wrap + add signature with options. */
fun Envelope.signOpt(signer: Signer, options: SigningOptions?): Envelope =
    wrap().addSignatureOpt(signer, options)

/** Convenience: verify signature + unwrap. */
fun Envelope.verify(verifier: Verifier): Envelope =
    verifySignatureFrom(verifier).unwrap()

/** Convenience: verify and return unwrapped envelope with metadata. */
fun Envelope.verifyReturningMetadata(verifier: Verifier): Pair<Envelope, Envelope> {
    val metadata = verifySignatureFromReturningMetadata(verifier)
    return Pair(unwrap(), metadata)
}
