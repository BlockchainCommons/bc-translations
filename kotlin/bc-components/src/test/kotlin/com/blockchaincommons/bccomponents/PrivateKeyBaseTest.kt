package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull

/**
 * Tests for [PrivateKeyBase].
 *
 * Based on Rust `private_key_base.rs` tests.
 */
class PrivateKeyBaseTest {

    private val seedHex = "59f2293a5bce7d4de59e71b4207ac5d2"

    @Test
    fun testPrivateKeyBase() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)

        // Test signing key derivation
        val signingPrivKey = pkb.signingPrivateKey()
        assertNotNull(signingPrivKey)
        assertEquals(32, signingPrivKey.data().size)

        // Test agreement key derivation
        val x25519PrivKey = pkb.x25519PrivateKey()
        assertNotNull(x25519PrivKey)

        // Test Ed25519 key derivation
        val ed25519PrivKey = pkb.ed25519SigningPrivateKey()
        assertNotNull(ed25519PrivKey)
        assertEquals(32, ed25519PrivKey.data().size)
    }

    @Test
    fun testPublicKeys() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)

        val publicKeys = pkb.publicKeys()
        assertNotNull(publicKeys.signingPublicKey)
        assertNotNull(publicKeys.encapsulationPublicKey)
    }

    @Test
    fun testPrivateKeys() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)

        val privateKeys = pkb.privateKeys()
        assertNotNull(privateKeys)
    }

    @Test
    fun testCborRoundtrip() {
        registerTags()
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)

        val cbor = pkb.taggedCbor()
        val decoded = PrivateKeyBase.fromTaggedCbor(cbor)
        assertEquals(pkb, decoded)
    }

    @Test
    fun testDataRoundtrip() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)

        val data = pkb.data()
        val restored = PrivateKeyBase.fromData(data)
        assertEquals(pkb, restored)
    }

    @Test
    fun testDeterministicDerivation() {
        val seedData = seedHex.hexToByteArray()
        val pkb1 = PrivateKeyBase.fromData(seedData)
        val pkb2 = PrivateKeyBase.fromData(seedData)

        // Same seed produces same signing key
        assertEquals(pkb1.signingPrivateKey(), pkb2.signingPrivateKey())

        // Same seed produces same agreement key
        assertEquals(pkb1.x25519PrivateKey(), pkb2.x25519PrivateKey())

        // Same seed produces same Ed25519 key
        assertEquals(pkb1.ed25519SigningPrivateKey(), pkb2.ed25519SigningPrivateKey())
    }
}
