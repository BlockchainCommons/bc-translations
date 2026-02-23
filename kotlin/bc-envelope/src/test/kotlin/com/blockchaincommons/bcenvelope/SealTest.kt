package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.EncapsulationScheme
import com.blockchaincommons.bccomponents.SignatureScheme
// registerTags() from bc-envelope package initializes GlobalFormatContext
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class SealTest {

    @Test
    fun testSealAndUnseal() {
        registerTags()

        // Create a test envelope
        val message = "Top secret message"
        val originalEnvelope = Envelope.from(message)

        // Generate keys for sender and recipient using established schemes
        val (senderPrivate, senderPublic) = SignatureScheme.Ed25519.keypair()
        val (recipientPrivate, recipientPublic) = EncapsulationScheme.X25519.keypair()

        // Step 1: Seal the envelope
        val sealedEnvelope = originalEnvelope.seal(senderPrivate, recipientPublic)

        // Verify the envelope is encrypted
        assertTrue(sealedEnvelope.isSubjectEncrypted())

        // Step 2: Unseal the envelope
        val unsealedEnvelope = sealedEnvelope.unseal(senderPublic, recipientPrivate)

        // Verify we got back the original message
        val extractedMessage = unsealedEnvelope.extractSubject<String>()
        assertEquals(message, extractedMessage)
    }

    @Test
    fun testSealOptWithOptions() {
        registerTags()

        // Create a test envelope
        val message = "Confidential data"
        val originalEnvelope = Envelope.from(message)

        // Generate keys for sender and recipient
        val (senderPrivate, senderPublic) = SignatureScheme.Ed25519.keypair()
        val (recipientPrivate, recipientPublic) = EncapsulationScheme.X25519.keypair()

        // Seal the envelope (no SSH options available in Kotlin, but we test the path)
        val sealedEnvelope = originalEnvelope.sealOpt(senderPrivate, recipientPublic)

        // Verify the envelope is encrypted
        assertTrue(sealedEnvelope.isSubjectEncrypted())

        // Unseal the envelope
        val unsealedEnvelope = sealedEnvelope.unseal(senderPublic, recipientPrivate)

        // Verify we got back the original message
        val extractedMessage = unsealedEnvelope.extractSubject<String>()
        assertEquals(message, extractedMessage)
    }
}
