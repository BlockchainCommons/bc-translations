package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Compressed
import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.EncryptedMessage
import com.blockchaincommons.knownvalues.KnownValue

/**
 * The core structural variants of a Gordian Envelope.
 *
 * Each variant represents a different structural form that an envelope can take,
 * as defined in the Gordian Envelope IETF Internet Draft.
 */
sealed class EnvelopeCase {
    /**
     * An envelope with a subject and one or more assertions.
     *
     * The digest is derived from the digests of the subject and all assertions.
     */
    data class Node(
        val subject: Envelope,
        val assertions: List<Envelope>,
        val digest: Digest,
    ) : EnvelopeCase()

    /**
     * An envelope containing a primitive CBOR value.
     *
     * The digest is derived directly from the CBOR representation.
     */
    data class Leaf(
        val cbor: com.blockchaincommons.dcbor.Cbor,
        val digest: Digest,
    ) : EnvelopeCase()

    /**
     * An envelope that wraps another envelope.
     *
     * The digest is derived from the digest of the wrapped envelope.
     */
    data class Wrapped(
        val envelope: Envelope,
        val digest: Digest,
    ) : EnvelopeCase()

    /**
     * A predicate-object assertion.
     */
    data class AssertionCase(
        val assertion: Assertion,
    ) : EnvelopeCase()

    /**
     * An envelope that has been elided, leaving only its digest.
     */
    data class Elided(
        val digest: Digest,
    ) : EnvelopeCase()

    /**
     * A value from a namespace of unsigned integers used for ontological concepts.
     */
    data class KnownValueCase(
        val value: KnownValue,
        val digest: Digest,
    ) : EnvelopeCase()

    /**
     * An envelope that has been encrypted.
     */
    data class Encrypted(
        val encryptedMessage: EncryptedMessage,
    ) : EnvelopeCase()

    /**
     * An envelope that has been compressed.
     */
    data class CompressedCase(
        val compressed: Compressed,
    ) : EnvelopeCase()
}
