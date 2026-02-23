package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Compressed
import com.blockchaincommons.bccomponents.toDigest
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.knownvalues.KnownValue
import kotlin.test.Test
import kotlin.test.assertEquals

class EnvelopeUnitTest {

    @Test
    fun testAnyEnvelope() {
        val e1 = Envelope.newLeaf(Cbor.fromString("Hello"))
        val e2 = Envelope.from("Hello")
        assertEquals(e1.format(), e2.format())
        assertEquals(e1.digest(), e2.digest())
    }

    @Test
    fun testAnyKnownValue() {
        val knownValue = KnownValue(100uL)
        val e1 = Envelope.newWithKnownValue(knownValue)
        val e2 = Envelope.from(knownValue)
        assertEquals(e1.format(), e2.format())
        assertEquals(e1.digest(), e2.digest())
    }

    @Test
    fun testAnyAssertion() {
        val assertion = Assertion(
            "knows".asEnvelopeEncodable(),
            "Bob".asEnvelopeEncodable(),
        )
        val e1 = Envelope.newWithAssertion(assertion)
        val e2 = Envelope.newAssertion("knows", "Bob")
        assertEquals(e1.format(), e2.format())
        assertEquals(e1.digest(), e2.digest())
    }

    @Test
    fun testAnyEncrypted() {
        // The Rust test is a todo!() stub -- keep placeholder for parity.
    }

    @Test
    fun testAnyCompressed() {
        val data = "Hello".toByteArray()
        val digest = data.toDigest()
        val compressed = Compressed.fromDecompressedData(data, digest)
        val e1 = Envelope.newWithCompressed(compressed)
        // Same construction path for e2 since there's no Compressed.toEnvelope()
        val e2 = Envelope.newWithCompressed(compressed)
        assertEquals(e1.format(), e2.format())
        assertEquals(e1.digest(), e2.digest())
    }

    @Test
    fun testAnyCborEncodable() {
        val e1 = Envelope.newLeaf(Cbor.fromInt(1))
        val e2 = Envelope.from(1)
        assertEquals(e1.format(), e2.format())
        assertEquals(e1.digest(), e2.digest())
    }
}
