package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.EncapsulationScheme
// registerTags() from bc-envelope package initializes GlobalFormatContext
import kotlin.test.Test
import kotlin.test.assertEquals

class EncapsulationTest {

    private fun testScheme(scheme: EncapsulationScheme) {
        val (privateKey, publicKey) = scheme.keypair()
        val envelope = helloEnvelope()
        val encryptedEnvelope = envelope
            .encryptToRecipient(publicKey)
            .checkEncoding()
        val decryptedEnvelope = encryptedEnvelope
            .decryptToRecipient(privateKey)
        assertEquals(
            envelope.structuralDigest(),
            decryptedEnvelope.structuralDigest()
        )
    }

    @Test
    fun testEncapsulation() {
        registerTags()

        testScheme(EncapsulationScheme.X25519)
        testScheme(EncapsulationScheme.MLKEM512)
        testScheme(EncapsulationScheme.MLKEM768)
        testScheme(EncapsulationScheme.MLKEM1024)
    }
}
