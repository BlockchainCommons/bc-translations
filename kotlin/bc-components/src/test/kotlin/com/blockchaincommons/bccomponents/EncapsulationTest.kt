package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * Tests for the unified encapsulation API.
 *
 * Covers keypair generation, encapsulation/decapsulation, and sealed message
 * encrypt/decrypt for X25519 and all ML-KEM security levels through the
 * [EncapsulationScheme] abstraction. Based on Rust `encapsulation/mod.rs`
 * tests.
 */
class EncapsulationTest {

    @Test
    fun testX25519Encapsulation() {
        val (privKey, pubKey) = EncapsulationScheme.X25519.keypair()

        val (sharedKey, ct) = pubKey.encapsulateNewSharedSecret()
        val recovered = privKey.decapsulateSharedSecret(ct)
        assertEquals(sharedKey, recovered)
    }

    @Test
    fun testMlkem512Encapsulation() {
        val (privKey, pubKey) = EncapsulationScheme.MLKEM512.keypair()

        val (sharedKey, ct) = pubKey.encapsulateNewSharedSecret()
        val recovered = privKey.decapsulateSharedSecret(ct)
        assertEquals(sharedKey, recovered)
    }

    @Test
    fun testMlkem768Encapsulation() {
        val (privKey, pubKey) = EncapsulationScheme.MLKEM768.keypair()

        val (sharedKey, ct) = pubKey.encapsulateNewSharedSecret()
        val recovered = privKey.decapsulateSharedSecret(ct)
        assertEquals(sharedKey, recovered)
    }

    @Test
    fun testMlkem1024Encapsulation() {
        val (privKey, pubKey) = EncapsulationScheme.MLKEM1024.keypair()

        val (sharedKey, ct) = pubKey.encapsulateNewSharedSecret()
        val recovered = privKey.decapsulateSharedSecret(ct)
        assertEquals(sharedKey, recovered)
    }

    @Test
    fun testSealedMessage() {
        registerTags()
        val plaintext = "Hello, World!".toByteArray()
        val (privKey, pubKey) = EncapsulationScheme.X25519.keypair()

        val sealed = SealedMessage.create(plaintext, pubKey)
        val decrypted = sealed.decrypt(privKey)
        assertTrue(plaintext.contentEquals(decrypted))
    }

    @Test
    fun testSealedMessageMlkem() {
        registerTags()
        val plaintext = "Hello, World!".toByteArray()
        val (privKey, pubKey) = EncapsulationScheme.MLKEM768.keypair()

        val sealed = SealedMessage.create(plaintext, pubKey)
        val decrypted = sealed.decrypt(privKey)
        assertTrue(plaintext.contentEquals(decrypted))
    }
}
