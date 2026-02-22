package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/**
 * Tests for ML-DSA post-quantum digital signature algorithm.
 *
 * Covers keypair generation, signing, verification, modified-message rejection,
 * and CBOR roundtrip for all three security levels (ML-DSA-44, ML-DSA-65,
 * ML-DSA-87). Based on Rust `mldsa/mod.rs` tests.
 */
class MLDSATest {
    private val message = ("Ladies and Gentlemen of the class of '99: " +
        "If I could offer you only one tip for the future, sunscreen would be it.")
        .toByteArray()

    @Test
    fun testMldsa44Signing() {
        val (privKey, pubKey) = MLDSA.MLDSA44.keypair()
        assertEquals(MLDSA.MLDSA44.privateKeySize(), privKey.data().size)
        assertEquals(MLDSA.MLDSA44.publicKeySize(), pubKey.data().size)

        val sig = privKey.sign(message)
        assertEquals(MLDSA.MLDSA44.signatureSize(), sig.data().size)
        assertTrue(pubKey.verify(sig, message))

        // Modified message should fail verification
        val modified = message.copyOf().also { it[0] = (it[0] + 1).toByte() }
        assertFalse(pubKey.verify(sig, modified))
    }

    @Test
    fun testMldsa65Signing() {
        val (privKey, pubKey) = MLDSA.MLDSA65.keypair()
        assertEquals(MLDSA.MLDSA65.privateKeySize(), privKey.data().size)
        assertEquals(MLDSA.MLDSA65.publicKeySize(), pubKey.data().size)

        val sig = privKey.sign(message)
        assertEquals(MLDSA.MLDSA65.signatureSize(), sig.data().size)
        assertTrue(pubKey.verify(sig, message))

        // Modified message should fail verification
        val modified = message.copyOf().also { it[0] = (it[0] + 1).toByte() }
        assertFalse(pubKey.verify(sig, modified))
    }

    @Test
    fun testMldsa87Signing() {
        val (privKey, pubKey) = MLDSA.MLDSA87.keypair()
        assertEquals(MLDSA.MLDSA87.privateKeySize(), privKey.data().size)
        assertEquals(MLDSA.MLDSA87.publicKeySize(), pubKey.data().size)

        val sig = privKey.sign(message)
        assertEquals(MLDSA.MLDSA87.signatureSize(), sig.data().size)
        assertTrue(pubKey.verify(sig, message))

        // Modified message should fail verification
        val modified = message.copyOf().also { it[0] = (it[0] + 1).toByte() }
        assertFalse(pubKey.verify(sig, modified))
    }

    @Test
    fun testMldsaCborRoundtrip() {
        registerTags()
        val (privKey, pubKey) = MLDSA.MLDSA65.keypair()
        val sig = privKey.sign(message)

        // Private key roundtrip
        val privDecoded = MLDSAPrivateKey.fromTaggedCbor(privKey.taggedCbor())
        assertEquals(privKey.level, privDecoded.level)
        assertTrue(privKey.data().contentEquals(privDecoded.data()))

        // Public key roundtrip
        val pubDecoded = MLDSAPublicKey.fromTaggedCbor(pubKey.taggedCbor())
        assertEquals(pubKey.level, pubDecoded.level)
        assertTrue(pubKey.data().contentEquals(pubDecoded.data()))

        // Signature roundtrip
        val sigDecoded = MLDSASignature.fromTaggedCbor(sig.taggedCbor())
        assertEquals(sig.level, sigDecoded.level)
        assertTrue(sig.data().contentEquals(sigDecoded.data()))
    }
}
