package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.SignatureScheme
// registerTags() from bc-envelope package initializes GlobalFormatContext
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith

class Ed25519Test {

    @Test
    fun testEd25519SignedPlaintext() {
        registerTags()

        // Generate Ed25519 keypair (returns SigningPrivateKey and SigningPublicKey
        // which implement Signer and Verifier respectively)
        val (alicePrivate, alicePublic) = SignatureScheme.Ed25519.keypair()

        // Alice sends a signed plaintext message to Bob.
        val envelope = helloEnvelope()
            .addSignature(alicePrivate)
            .checkEncoding()
        val ur = envelope.ur()

        val expectedFormat = """
            "Hello." [
                'signed': Signature(Ed25519)
            ]
        """.trimIndent()
        assertEquals(expectedFormat, envelope.format())

        // Bob receives the envelope.
        val receivedEnvelope = Envelope.fromUr(ur).checkEncoding()

        // Bob validates Alice's signature and reads the message.
        val receivedPlaintext = receivedEnvelope
            .verifySignatureFrom(alicePublic)
            .extractSubject<String>()
        assertEquals("Hello.", receivedPlaintext)

        // Generate Carol's Ed25519 keypair for negative testing.
        val (_, carolPublic) = SignatureScheme.Ed25519.keypair()

        // Confirm that it wasn't signed by Carol.
        assertFailsWith<EnvelopeException> {
            receivedEnvelope.verifySignatureFrom(carolPublic)
        }

        // Confirm that it was signed by Alice OR Carol.
        receivedEnvelope.verifySignaturesFromThreshold(
            listOf(alicePublic, carolPublic),
            1,
        )

        // Confirm that it was not signed by Alice AND Carol.
        assertFailsWith<EnvelopeException> {
            receivedEnvelope.verifySignaturesFromThreshold(
                listOf(alicePublic, carolPublic),
                2,
            )
        }
    }
}
