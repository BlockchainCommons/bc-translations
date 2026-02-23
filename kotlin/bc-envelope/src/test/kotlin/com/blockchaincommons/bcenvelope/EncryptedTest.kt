@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class EncryptedTest {

    private val symmetricKey = SymmetricKey.fromData(
        "38900719dea655e9a1bc1682aaccf0bfcd79a7239db672d39216e4acdd660dc0".hexToByteArray()
    )

    private val testNonce = Nonce.fromData(
        "4d785658f36c22fb5aed3ac0".hexToByteArray()
    )

    private fun encryptedTest(e1: Envelope) {
        val e2 = e1
            .encryptSubject(symmetricKey, testNonce)
            .checkEncoding()

        assertTrue(e1.isEquivalentTo(e2))
        assertTrue(e1.subject().isEquivalentTo(e2.subject()))

        val encryptedMessage = e2.extractSubject<EncryptedMessage>()
        assertEquals(encryptedMessage.digest(), e1.subject().digest())

        val e3 = e2.decryptSubject(symmetricKey)
        assertTrue(e1.isEquivalentTo(e3))
    }

    @Test
    fun testEncrypted() {
        encryptedTest(Envelope.from("Hello."))
        encryptedTest(Envelope.from("Hello.").wrap())
        encryptedTest(Envelope.from("Hello.").wrap().wrap())
        encryptedTest(knownValueEnvelope())
        encryptedTest(assertionEnvelope())
        encryptedTest(singleAssertionEnvelope())
        encryptedTest(doubleAssertionEnvelope())
    }
}
