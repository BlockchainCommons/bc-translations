package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals

/**
 * Tests for ML-KEM post-quantum key encapsulation mechanism.
 *
 * Covers keypair generation, encapsulation/decapsulation, and key size
 * validation for all three security levels (ML-KEM-512, ML-KEM-768,
 * ML-KEM-1024). Based on Rust `mlkem/mod.rs` tests.
 */
class MLKEMTest {

    @Test
    fun testMlkem512() {
        val (privKey, pubKey) = MLKEM.MLKEM512.keypair()
        assertEquals(1632, privKey.data().size)
        assertEquals(800, pubKey.data().size)

        val (sharedSecret, ciphertext) = pubKey.encapsulateNewSharedSecret()
        assertEquals(768, ciphertext.data().size)
        assertEquals(32, sharedSecret.data().size)

        val recovered = privKey.decapsulateSharedSecret(ciphertext)
        assertEquals(sharedSecret, recovered)
    }

    @Test
    fun testMlkem768() {
        val (privKey, pubKey) = MLKEM.MLKEM768.keypair()
        assertEquals(2400, privKey.data().size)
        assertEquals(1184, pubKey.data().size)

        val (sharedSecret, ciphertext) = pubKey.encapsulateNewSharedSecret()
        assertEquals(1088, ciphertext.data().size)
        assertEquals(32, sharedSecret.data().size)

        val recovered = privKey.decapsulateSharedSecret(ciphertext)
        assertEquals(sharedSecret, recovered)
    }

    @Test
    fun testMlkem1024() {
        val (privKey, pubKey) = MLKEM.MLKEM1024.keypair()
        assertEquals(3168, privKey.data().size)
        assertEquals(1568, pubKey.data().size)

        val (sharedSecret, ciphertext) = pubKey.encapsulateNewSharedSecret()
        assertEquals(1568, ciphertext.data().size)
        assertEquals(32, sharedSecret.data().size)

        val recovered = privKey.decapsulateSharedSecret(ciphertext)
        assertEquals(sharedSecret, recovered)
    }
}
