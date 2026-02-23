package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.DigestProvider

/**
 * Extension for round-trip CBOR encoding verification in tests.
 */
fun Envelope.checkEncoding(): Envelope {
    val cbor = this.taggedCbor()
    val restored = try {
        Envelope.fromTaggedCbor(cbor)
    } catch (e: Exception) {
        println("=== EXPECTED")
        println(this.format())
        println("=== GOT")
        println(cbor.diagnostic())
        println("===")
        throw EnvelopeException.InvalidFormat()
    }
    if (this.digest() != restored.digest()) {
        println("=== EXPECTED")
        println(this.format())
        println("=== GOT")
        println(restored.format())
        println("===")
        throw EnvelopeException.General("Digest mismatch")
    }
    return this
}
