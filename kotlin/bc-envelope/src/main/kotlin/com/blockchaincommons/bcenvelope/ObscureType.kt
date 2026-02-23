package com.blockchaincommons.bcenvelope

/**
 * The type of obscuration applied to an envelope element.
 *
 * Used by [Envelope.nodesMatching] to filter for elements obscured in
 * a particular way.
 */
enum class ObscureType {
    /** The element has been elided (replaced by its digest). */
    Elided,

    /** The element has been encrypted. */
    Encrypted,

    /** The element has been compressed. */
    Compressed,
}
