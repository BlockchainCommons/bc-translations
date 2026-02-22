package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNotEquals

/**
 * Tests for [Nonce].
 *
 * Based on Rust `nonce.rs` inline tests: test_nonce_raw, test_nonce_from_raw_data,
 * test_nonce_size, test_nonce_new, test_nonce_hex_roundtrip, and
 * test_nonce_cbor_roundtrip.
 */
class NonceTest {

    @Test
    fun testNonceRaw() {
        val nonceRaw = ByteArray(Nonce.NONCE_SIZE)
        val nonce = Nonce.fromData(nonceRaw)
        assertContentEquals(nonceRaw, nonce.data())
    }

    @Test
    fun testNonceFromRawData() {
        val rawData = ByteArray(Nonce.NONCE_SIZE)
        val nonce = Nonce.fromDataChecked(rawData)
        assertContentEquals(rawData, nonce.data())
    }

    @Test
    fun testNonceSize() {
        val rawData = ByteArray(Nonce.NONCE_SIZE + 1)
        assertFailsWith<BcComponentsException> {
            Nonce.fromDataChecked(rawData)
        }
    }

    @Test
    fun testNonceNew() {
        val nonce1 = Nonce.create()
        val nonce2 = Nonce.create()
        assertNotEquals(nonce1, nonce2)
    }

    @Test
    fun testNonceHexRoundtrip() {
        val nonce = Nonce.create()
        val hexString = nonce.hex
        val nonceFromHex = Nonce.fromHex(hexString)
        assertEquals(nonce, nonceFromHex)
    }

    @Test
    fun testNonceCborRoundtrip() {
        registerTags()
        val nonce = Nonce.create()
        val cbor = nonce.taggedCbor()
        val decoded = Nonce.fromTaggedCbor(cbor)
        assertEquals(nonce, decoded)
    }
}
