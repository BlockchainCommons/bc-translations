package com.blockchaincommons.bcenvelope

/**
 * Error types returned when operating on Gordian Envelopes.
 *
 * These errors capture various conditions that can occur when working with
 * envelopes, including structure validation, operation constraints, and
 * extension-specific errors.
 */
sealed class EnvelopeException(message: String) : Exception(message) {

    // -- Base Specification --

    /** Envelope was elided, so it cannot be compressed or encrypted. */
    class AlreadyElided : EnvelopeException("envelope was elided, so it cannot be compressed or encrypted")

    /** More than one assertion matches the predicate. */
    class AmbiguousPredicate : EnvelopeException("more than one assertion matches the predicate")

    /** Digest did not match. */
    class InvalidDigest : EnvelopeException("digest did not match")

    /** Invalid format. */
    class InvalidFormat : EnvelopeException("invalid format")

    /** A digest was expected but not found. */
    class MissingDigest : EnvelopeException("a digest was expected but not found")

    /** No assertion matches the predicate. */
    class NonexistentPredicate : EnvelopeException("no assertion matches the predicate")

    /** Cannot unwrap an envelope that was not wrapped. */
    class NotWrapped : EnvelopeException("cannot unwrap an envelope that was not wrapped")

    /** The envelope's subject is not a leaf. */
    class NotLeaf : EnvelopeException("the envelope's subject is not a leaf")

    /** The envelope's subject is not an assertion. */
    class NotAssertion : EnvelopeException("the envelope's subject is not an assertion")

    /** Assertion must be a map with exactly one element. */
    class InvalidAssertion : EnvelopeException("assertion must be a map with exactly one element")

    // -- Attachments Extension --

    /** Invalid attachment. */
    class InvalidAttachment : EnvelopeException("invalid attachment")

    /** Nonexistent attachment. */
    class NonexistentAttachment : EnvelopeException("nonexistent attachment")

    /** Ambiguous attachment. */
    class AmbiguousAttachment : EnvelopeException("ambiguous attachment")

    // -- Edges Extension --

    /** Edge missing 'isA' assertion. */
    class EdgeMissingIsA : EnvelopeException("edge missing 'isA' assertion")

    /** Edge missing 'source' assertion. */
    class EdgeMissingSource : EnvelopeException("edge missing 'source' assertion")

    /** Edge missing 'target' assertion. */
    class EdgeMissingTarget : EnvelopeException("edge missing 'target' assertion")

    /** Edge has duplicate 'isA' assertions. */
    class EdgeDuplicateIsA : EnvelopeException("edge has duplicate 'isA' assertions")

    /** Edge has duplicate 'source' assertions. */
    class EdgeDuplicateSource : EnvelopeException("edge has duplicate 'source' assertions")

    /** Edge has duplicate 'target' assertions. */
    class EdgeDuplicateTarget : EnvelopeException("edge has duplicate 'target' assertions")

    /** Edge has unexpected assertion. */
    class EdgeUnexpectedAssertion : EnvelopeException("edge has unexpected assertion")

    /** Nonexistent edge. */
    class NonexistentEdge : EnvelopeException("nonexistent edge")

    /** Ambiguous edge. */
    class AmbiguousEdge : EnvelopeException("ambiguous edge")

    // -- Compression Extension --

    /** Envelope was already compressed. */
    class AlreadyCompressed : EnvelopeException("envelope was already compressed")

    /** Cannot decompress an envelope that was not compressed. */
    class NotCompressed : EnvelopeException("cannot decompress an envelope that was not compressed")

    // -- Symmetric Encryption Extension --

    /** Envelope was already encrypted or compressed, so it cannot be encrypted. */
    class AlreadyEncrypted : EnvelopeException("envelope was already encrypted or compressed, so it cannot be encrypted")

    /** Cannot decrypt an envelope that was not encrypted. */
    class NotEncrypted : EnvelopeException("cannot decrypt an envelope that was not encrypted")

    // -- Known Values Extension --

    /** The envelope's subject is not a known value. */
    class NotKnownValue : EnvelopeException("the envelope's subject is not a known value")

    // -- Public Key Encryption Extension --

    /** Unknown recipient. */
    class UnknownRecipient : EnvelopeException("unknown recipient")

    // -- Encrypted Key Extension --

    /** Secret not found. */
    class UnknownSecret : EnvelopeException("secret not found")

    // -- Public Key Signing Extension --

    /** Could not verify a signature. */
    class UnverifiedSignature : EnvelopeException("could not verify a signature")

    /** Unexpected outer signature object type. */
    class InvalidOuterSignatureType : EnvelopeException("unexpected outer signature object type")

    /** Unexpected inner signature object type. */
    class InvalidInnerSignatureType : EnvelopeException("unexpected inner signature object type")

    /** Inner signature not made with same key as outer signature. */
    class UnverifiedInnerSignature : EnvelopeException("inner signature not made with same key as outer signature")

    /** Unexpected signature object type. */
    class InvalidSignatureType : EnvelopeException("unexpected signature object type")

    // -- SSKR Extension --

    /** Invalid SSKR shares. */
    class InvalidShares : EnvelopeException("invalid SSKR shares")

    // -- Types Extension --

    /** Invalid type. */
    class InvalidType : EnvelopeException("invalid type")

    /** Ambiguous type. */
    class AmbiguousType : EnvelopeException("ambiguous type")

    /** The subject of the envelope is not the unit value. */
    class SubjectNotUnit : EnvelopeException("the subject of the envelope is not the unit value")

    // -- Expressions Extension --

    /** Unexpected response ID. */
    class UnexpectedResponseID : EnvelopeException("unexpected response ID")

    /** Invalid response. */
    class InvalidResponse : EnvelopeException("invalid response")

    // -- Wrapped errors --

    /** SSKR error. */
    class SSKR(cause: Exception) : EnvelopeException("sskr error: ${cause.message}")

    /** CBOR error. */
    class Cbor(cause: Exception) : EnvelopeException("cbor error: ${cause.message}")

    /** Components error. */
    class Components(cause: Exception) : EnvelopeException("components error: ${cause.message}")

    /** General error. */
    class General(msg: String) : EnvelopeException("general error: $msg")

    companion object {
        /** Creates a general error with the given message. */
        fun msg(message: String): EnvelopeException = General(message)
    }
}
