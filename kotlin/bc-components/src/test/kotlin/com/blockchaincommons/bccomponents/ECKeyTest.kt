package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * Tests for EC key operations: [ECPrivateKey], [ECPublicKey],
 * [SchnorrPublicKey], [ECUncompressedPublicKey], [Ed25519PrivateKey],
 * and [Ed25519PublicKey].
 *
 * Based on Rust EC key tests.
 */
class ECKeyTest {

    private val privateKeyHex = "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"

    @Test
    fun testEcPrivateKeyFromHex() {
        val key = ECPrivateKey.fromHex(privateKeyHex)
        assertEquals(privateKeyHex, key.hex)
        assertEquals(32, key.data().size)
    }

    @Test
    fun testEcPublicKeyDerivation() {
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val pubKey = privKey.publicKey()
        assertEquals(33, pubKey.data().size)
    }

    @Test
    fun testSchnorrPublicKeyDerivation() {
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val schnorrPubKey = privKey.schnorrPublicKey()
        assertEquals(32, schnorrPubKey.data().size)
    }

    @Test
    fun testEcdsaSignAndVerify() {
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val pubKey = privKey.publicKey()
        val message = "Hello, World!".toByteArray()

        val signature = privKey.ecdsaSign(message)
        assertEquals(64, signature.size)
        assertTrue(pubKey.verify(signature, message))
    }

    @Test
    fun testSchnorrSignAndVerify() {
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val schnorrPubKey = privKey.schnorrPublicKey()
        val message = "Hello, World!".toByteArray()

        val signature = privKey.schnorrSign(message)
        assertEquals(64, signature.size)
        assertTrue(schnorrPubKey.schnorrVerify(signature, message))
    }

    @Test
    fun testEcPrivateKeyCborRoundtrip() {
        registerTags()
        val key = ECPrivateKey.fromHex(privateKeyHex)
        val cbor = key.taggedCbor()
        val decoded = ECPrivateKey.fromTaggedCbor(cbor)
        assertEquals(key, decoded)
    }

    @Test
    fun testEcPublicKeyCborRoundtrip() {
        registerTags()
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val pubKey = privKey.publicKey()
        val cbor = pubKey.taggedCbor()
        val decoded = ECPublicKey.fromTaggedCbor(cbor)
        assertEquals(pubKey, decoded)
    }

    @Test
    fun testUncompressedPublicKey() {
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val pubKey = privKey.publicKey()
        val uncompressed = pubKey.uncompressedPublicKey()
        assertEquals(65, uncompressed.data().size)
    }

    @Test
    fun testUncompressedToCompressedRoundtrip() {
        val privKey = ECPrivateKey.fromHex(privateKeyHex)
        val pubKey = privKey.publicKey()
        val uncompressed = pubKey.uncompressedPublicKey()
        val recompressed = uncompressed.publicKey()
        assertEquals(pubKey, recompressed)
    }

    @Test
    fun testEd25519SignAndVerify() {
        val privKey = Ed25519PrivateKey.create()
        val pubKey = privKey.publicKey()
        val message = "Hello, World!".toByteArray()

        val signature = privKey.sign(message)
        assertEquals(64, signature.size)
        assertTrue(pubKey.verify(signature, message))
    }

    @Test
    fun testEd25519DeterministicDerivation() {
        val keyMaterial = "test-key-material".toByteArray()
        val key1 = Ed25519PrivateKey.deriveFromKeyMaterial(keyMaterial)
        val key2 = Ed25519PrivateKey.deriveFromKeyMaterial(keyMaterial)
        assertEquals(key1, key2)
        assertEquals(key1.publicKey(), key2.publicKey())
    }

    @Test
    fun testEcPrivateKeyDeterministicDerivation() {
        val keyMaterial = "test-key-material".toByteArray()
        val key1 = ECPrivateKey.deriveFromKeyMaterial(keyMaterial)
        val key2 = ECPrivateKey.deriveFromKeyMaterial(keyMaterial)
        assertEquals(key1, key2)
        assertEquals(key1.publicKey(), key2.publicKey())
    }
}
