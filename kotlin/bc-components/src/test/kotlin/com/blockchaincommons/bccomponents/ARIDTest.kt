package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNotEquals

/**
 * Tests for [ARID].
 *
 * Tests cover creation, hex roundtrip, CBOR roundtrip, UR roundtrip,
 * comparison, and error handling. The Rust `id/arid.rs` module has no inline
 * tests; these tests validate the Kotlin translation against the Rust API
 * surface and the ARID specification (BCR-2022-002).
 */
class ARIDTest {

    @Test
    fun testCreate() {
        val arid = ARID.create()
        assertEquals(ARID.ARID_SIZE, arid.data().size)
    }

    @Test
    fun testUniqueness() {
        val arid1 = ARID.create()
        val arid2 = ARID.create()
        assertNotEquals(arid1, arid2)
    }

    @Test
    fun testFromData() {
        val data = ByteArray(ARID.ARID_SIZE) { it.toByte() }
        val arid = ARID.fromData(data)
        assertEquals(ARID.ARID_SIZE, arid.data().size)
    }

    @Test
    fun testInvalidSize() {
        val data = ByteArray(16)
        assertFailsWith<BcComponentsException> {
            ARID.fromData(data)
        }
    }

    @Test
    fun testHexRoundtrip() {
        val arid = ARID.create()
        val hex = arid.hex
        assertEquals(64, hex.length) // 32 bytes = 64 hex chars
        val arid2 = ARID.fromHex(hex)
        assertEquals(arid, arid2)
    }

    @Test
    fun testShortDescription() {
        val data = ByteArray(ARID.ARID_SIZE) { it.toByte() }
        val arid = ARID.fromData(data)
        // Short description is first 4 bytes as hex
        assertEquals("00010203", arid.shortDescription())
    }

    @Test
    fun testCborRoundtrip() {
        registerTags()
        val arid = ARID.create()
        val cbor = arid.taggedCbor()
        val decoded = ARID.fromTaggedCbor(cbor)
        assertEquals(arid, decoded)
    }

    @Test
    fun testUrRoundtrip() {
        registerTags()
        val arid = ARID.create()
        val ur = arid.ur()
        val decoded = ARID.fromUr(ur)
        assertEquals(arid, decoded)
    }

    @Test
    fun testEquality() {
        val hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        val arid1 = ARID.fromHex(hex)
        val arid2 = ARID.fromHex(hex)
        assertEquals(arid1, arid2)
    }

    @Test
    fun testComparable() {
        val data1 = ByteArray(ARID.ARID_SIZE) { 0 }
        val data2 = ByteArray(ARID.ARID_SIZE) { 1 }
        val arid1 = ARID.fromData(data1)
        val arid2 = ARID.fromData(data2)
        assert(arid1 < arid2)
    }

    @Test
    fun testToString() {
        val hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        val arid = ARID.fromHex(hex)
        assertEquals("ARID($hex)", arid.toString())
    }
}
