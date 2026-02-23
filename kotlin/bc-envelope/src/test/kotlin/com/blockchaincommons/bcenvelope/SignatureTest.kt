package com.blockchaincommons.bcenvelope

import com.blockchaincommons.knownvalues.NOTE
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class SignatureTest {

    @Test
    fun testSignedPlaintext() {
        val envelope = helloEnvelope()
            .addSignature(alicePrivateKey())
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            "Hello." [
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedEnvelope = Envelope.fromUr(ur).checkEncoding()

        val receivedPlaintext = receivedEnvelope
            .verifySignatureFrom(alicePublicKey())
            .extractSubject<String>()
        assertEquals("Hello.", receivedPlaintext)

        // Confirm not signed by Carol
        assertTrue(
            try { receivedEnvelope.verifySignatureFrom(carolPublicKey()); false }
            catch (_: Exception) { true }
        )

        // Confirm signed by Alice OR Carol
        receivedEnvelope.verifySignaturesFromThreshold(
            listOf(alicePublicKey(), carolPublicKey()),
            1
        )

        // Confirm not signed by Alice AND Carol
        assertTrue(
            try {
                receivedEnvelope.verifySignaturesFromThreshold(
                    listOf(alicePublicKey(), carolPublicKey()),
                    2
                ); false
            } catch (_: Exception) { true }
        )
    }

    @Test
    fun testMultisignedPlaintext() {
        registerTags()
        val envelope = helloEnvelope()
            .addSignatures(listOf(alicePrivateKey(), carolPrivateKey()))
            .checkEncoding()

        assertEquals(
            """
            "Hello." [
                'signed': Signature
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        val ur = envelope.ur()
        val receivedPlaintext = Envelope.fromUr(ur)
            .checkEncoding()
            .verifySignaturesFrom(listOf(alicePublicKey(), carolPublicKey()))
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, receivedPlaintext)
    }

    @Test
    fun testSignedWithMetadata() {
        registerTags()

        val envelope = helloEnvelope()
        val metadata = SignatureMetadata().withAssertion(NOTE, "Alice signed this.")

        val signed = envelope
            .wrap()
            .addSignatureOpt(alicePrivateKey(), null, metadata)
            .checkEncoding()

        assertEquals(
            """
            {
                "Hello."
            } [
                'signed': {
                    Signature [
                        'note': "Alice signed this."
                    ]
                } [
                    'signed': Signature
                ]
            ]
            """.trimIndent(),
            signed.format()
        )

        val ur = signed.ur()
        val (received, meta) = Envelope.fromUr(ur)
            .checkEncoding()
            .verifyReturningMetadata(alicePublicKey())

        assertEquals(
            """
            Signature [
                'note': "Alice signed this."
            ]
            """.trimIndent(),
            meta.format()
        )

        val note = meta.objectForPredicate(NOTE)
            .extractSubject<String>()
        assertEquals("Alice signed this.", note)

        val receivedPlaintext = received.extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, receivedPlaintext)
    }
}
