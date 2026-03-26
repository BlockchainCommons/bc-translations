package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.Cbor

/** Multipart UR decoder using fountain codes. */
class MultipartDecoder {
    private var urType: URType? = null
    private val decoder = FountainDecoder()

    /**
     * Receives a multipart UR string.
     *
     * Accepts both lowercase and uppercase input.
     *
     * @throws URException if the string is malformed or the type is inconsistent
     */
    fun receive(value: String) {
        val lower = value.lowercase()
        val decodedType = decodeType(lower)
        val current = urType
        if (current != null) {
            if (current != decodedType) {
                throw URException.UnexpectedType(current.value, decodedType.value)
            }
        } else {
            urType = decodedType
        }

        val (kind, data) = UREncoding.decode(lower)
        if (kind != UREncoding.Kind.MultiPart) {
            throw URException.NotSinglePart()
        }
        val part = FountainPart.fromCbor(data)
        decoder.receive(part)
    }

    /** Returns whether the decoder has received all fragments. */
    val isComplete: Boolean get() = decoder.isComplete

    /** Number of fragments that have been fully decoded. */
    val decodedFragmentCount: Int get() = decoder.decodedCount

    /** Total number of fragments needed. Returns 0 before the first part is received. */
    val expectedFragmentCount: Int get() = decoder.expectedCount

    /** Set of fragment indexes that have been fully decoded. */
    val decodedFragmentIndexes: Set<Int> get() = decoder.decodedIndexes

    /** Partial progress credit from buffered mixed-degree parts. */
    val bufferContribution: Double get() = decoder.bufferContribution

    /** Returns the decoded UR if complete, or null. */
    fun message(): UR? {
        val data = decoder.message() ?: return null
        val cbor = Cbor.tryFromData(data)
        val type = urType!!
        return UR(type, cbor)
    }

    private fun decodeType(urString: String): URType {
        if (!urString.startsWith("ur:")) {
            throw URException.InvalidScheme()
        }
        val withoutScheme = urString.removePrefix("ur:")
        val firstComponent = withoutScheme.split('/').firstOrNull()
            ?: throw URException.InvalidType()
        return URType(firstComponent)
    }
}
