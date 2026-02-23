package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.KeyDerivationMethod
import com.blockchaincommons.bccomponents.SymmetricKey
import com.blockchaincommons.knownvalues.IS_A
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class CryptoTest {

    @Test
    fun testPlaintext() {
        registerTags()
        val envelope = helloEnvelope()
        val ur = envelope.ur()
        assertEquals("\"Hello.\"", envelope.format())

        val receivedPlaintext = Envelope.fromUr(ur)
            .checkEncoding()
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, receivedPlaintext)
    }

    @Test
    fun testSymmetricEncryption() {
        registerTags()
        val key = SymmetricKey.create()

        val envelope = helloEnvelope()
            .encryptSubject(key)
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals("ENCRYPTED", envelope.format())

        val receivedEnvelope = Envelope.fromUr(ur).checkEncoding()
        val receivedPlaintext = receivedEnvelope
            .decryptSubject(key)
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, receivedPlaintext)

        // Can't read with no key
        assertTrue(
            try { receivedEnvelope.extractSubject<String>(); false }
            catch (_: Exception) { true }
        )

        // Can't read with wrong key
        assertTrue(
            try { receivedEnvelope.decryptSubject(SymmetricKey.create()); false }
            catch (_: Exception) { true }
        )
    }

    private fun roundTripTest(envelope: Envelope) {
        val key = SymmetricKey.create()
        val plaintextSubject = envelope.checkEncoding()
        val encryptedSubject = plaintextSubject.encryptSubject(key)
        assertTrue(encryptedSubject.isEquivalentTo(plaintextSubject))
        val plaintextSubject2 = encryptedSubject
            .decryptSubject(key)
            .checkEncoding()
        assertTrue(encryptedSubject.isEquivalentTo(plaintextSubject2))
        assertTrue(plaintextSubject.isIdenticalTo(plaintextSubject2))
    }

    @Test
    fun testEncryptDecrypt() {
        // leaf
        roundTripTest(Envelope.from(PLAINTEXT_HELLO))
        // node
        roundTripTest(Envelope.from("Alice").addAssertion("knows", "Bob"))
        // wrapped
        roundTripTest(Envelope.from("Alice").wrap())
        // known value
        roundTripTest(IS_A.toEnvelope())
        // assertion
        roundTripTest(Envelope.newAssertion("knows", "Bob"))
        // compressed
        roundTripTest(Envelope.from(PLAINTEXT_HELLO).compress())
    }

    @Test
    fun testSignThenEncrypt() {
        registerTags()
        val key = SymmetricKey.create()

        val envelope = helloEnvelope()
            .addSignature(alicePrivateKey())
            .checkEncoding()
            .wrap()
            .checkEncoding()
            .encryptSubject(key)
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals("ENCRYPTED", envelope.format())

        val receivedPlaintext = Envelope.fromUr(ur)
            .checkEncoding()
            .decryptSubject(key)
            .checkEncoding()
            .unwrap()
            .checkEncoding()
            .verifySignatureFrom(alicePublicKey())
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, receivedPlaintext)
    }

    @Test
    fun testEncryptThenSign() {
        registerTags()
        val key = SymmetricKey.create()

        val envelope = helloEnvelope()
            .encryptSubject(key)
            .addSignature(alicePrivateKey())
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            ENCRYPTED [
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedPlaintext = Envelope.fromUr(ur)
            .checkEncoding()
            .verifySignatureFrom(alicePublicKey())
            .decryptSubject(key)
            .checkEncoding()
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, receivedPlaintext)
    }

    @Test
    fun testMultiRecipient() {
        val contentKey = SymmetricKey.create()
        val envelope = helloEnvelope()
            .encryptSubject(contentKey)
            .addRecipient(bobPublicKey(), contentKey)
            .addRecipient(carolPublicKey(), contentKey)
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedEnvelope = Envelope.fromUr(ur)

        // Bob decrypts
        val bobPlaintext = receivedEnvelope
            .decryptSubjectToRecipient(bobPrivateKey())
            .checkEncoding()
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, bobPlaintext)

        // Carol decrypts
        val carolPlaintext = receivedEnvelope
            .decryptSubjectToRecipient(carolPrivateKey())
            .checkEncoding()
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, carolPlaintext)

        // Alice can't decrypt
        assertTrue(
            try { receivedEnvelope.decryptSubjectToRecipient(alicePrivateKey()); false }
            catch (_: Exception) { true }
        )
    }

    @Test
    fun testVisibleSignatureMultiRecipient() {
        val contentKey = SymmetricKey.create()
        val envelope = helloEnvelope()
            .addSignature(alicePrivateKey())
            .encryptSubject(contentKey)
            .addRecipient(bobPublicKey(), contentKey)
            .addRecipient(carolPublicKey(), contentKey)
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
                'signed': Signature
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedEnvelope = Envelope.fromUr(ur)

        // Bob validates then decrypts
        val bobPlaintext = receivedEnvelope
            .verifySignatureFrom(alicePublicKey())
            .decryptSubjectToRecipient(bobPrivateKey())
            .checkEncoding()
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, bobPlaintext)
    }

    @Test
    fun testHiddenSignatureMultiRecipient() {
        val contentKey = SymmetricKey.create()
        val envelope = helloEnvelope()
            .addSignature(alicePrivateKey())
            .wrap()
            .encryptSubject(contentKey)
            .addRecipient(bobPublicKey(), contentKey)
            .addRecipient(carolPublicKey(), contentKey)
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedEnvelope = Envelope.fromUr(ur)

        // Bob decrypts, unwraps, then validates
        val bobPlaintext = receivedEnvelope
            .decryptSubjectToRecipient(bobPrivateKey())
            .unwrap()
            .checkEncoding()
            .verifySignatureFrom(alicePublicKey())
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, bobPlaintext)
    }

    @Test
    fun testSecret1() {
        registerTags()
        val bobPassword = "correct horse battery staple"
        val envelope = helloEnvelope()
            .lock(KeyDerivationMethod.HKDF, bobPassword)
        envelope.checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            ENCRYPTED [
                'hasSecret': EncryptedKey(HKDF(SHA256))
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedEnvelope = Envelope.fromUr(ur)
        val bobPlaintext = receivedEnvelope
            .unlock(bobPassword)
            .checkEncoding()
            .extractSubject<String>()
        assertEquals(PLAINTEXT_HELLO, bobPlaintext)

        assertTrue(
            try { receivedEnvelope.unlock("wrong password"); false }
            catch (_: Exception) { true }
        )
    }

    @Test
    fun testSecret2() {
        registerTags()
        val bobPassword = "correct horse battery staple"
        val carolPassword = "Able was I ere I saw Elba"
        val gracyPassword = "Madam, in Eden, I'm Adam"
        val contentKey = SymmetricKey.create()
        val envelope = helloEnvelope()
            .encryptSubject(contentKey)
            .addSecret(KeyDerivationMethod.HKDF, bobPassword, contentKey)
            .addSecret(KeyDerivationMethod.Scrypt, carolPassword, contentKey)
            .addSecret(KeyDerivationMethod.Argon2id, gracyPassword, contentKey)
            .checkEncoding()
        val ur = envelope.ur()

        assertEquals(
            """
            ENCRYPTED [
                'hasSecret': EncryptedKey(Argon2id)
                'hasSecret': EncryptedKey(HKDF(SHA256))
                'hasSecret': EncryptedKey(Scrypt)
            ]
            """.trimIndent(),
            envelope.format()
        )

        val receivedEnvelope = Envelope.fromUr(ur)
        assertEquals(
            PLAINTEXT_HELLO,
            receivedEnvelope.unlockSubject(bobPassword).checkEncoding().extractSubject<String>()
        )
        assertEquals(
            PLAINTEXT_HELLO,
            receivedEnvelope.unlockSubject(carolPassword).checkEncoding().extractSubject<String>()
        )
        assertEquals(
            PLAINTEXT_HELLO,
            receivedEnvelope.unlockSubject(gracyPassword).checkEncoding().extractSubject<String>()
        )
        assertTrue(
            try { receivedEnvelope.unlockSubject("wrong password"); false }
            catch (_: Exception) { true }
        )
    }
}
