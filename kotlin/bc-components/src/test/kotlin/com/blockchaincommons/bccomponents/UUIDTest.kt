package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotEquals
import kotlin.test.assertTrue

/**
 * Tests for [UUID].
 *
 * Tests cover creation, formatting, string parsing, CBOR roundtrip, and
 * type 4 UUID version/variant fields. The Rust `id/uuid.rs` module has no
 * inline tests; these tests validate the Kotlin translation against the
 * Rust API surface and UUID specification.
 */
class UUIDTest {

    @Test
    fun testCreate() {
        val uuid = UUID.create()
        assertEquals(UUID.UUID_SIZE, uuid.data().size)
    }

    @Test
    fun testUniqueness() {
        val uuid1 = UUID.create()
        val uuid2 = UUID.create()
        assertNotEquals(uuid1, uuid2)
    }

    @Test
    fun testVersion4() {
        // Type 4 UUIDs have version nibble = 4 in byte 6
        val uuid = UUID.create()
        val data = uuid.data()
        val versionNibble = (data[6].toInt() and 0xF0) ushr 4
        assertEquals(4, versionNibble, "UUID version must be 4")
    }

    @Test
    fun testVariant2() {
        // RFC 4122 variant: top two bits of byte 8 = 10
        val uuid = UUID.create()
        val data = uuid.data()
        val variantBits = (data[8].toInt() and 0xC0) ushr 6
        assertEquals(2, variantBits, "UUID variant must be 2 (RFC 4122)")
    }

    @Test
    fun testStringFormat() {
        val data = "0123456789abcdef0123456789abcdef".hexToByteArray()
        val uuid = UUID.fromData(data)
        val str = uuid.toString()
        // Canonical UUID format: 8-4-4-4-12
        assertTrue(str.matches(Regex("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}")))
        assertEquals("01234567-89ab-cdef-0123-456789abcdef", str)
    }

    @Test
    fun testFromString() {
        val uuidString = "01234567-89ab-cdef-0123-456789abcdef"
        val uuid = UUID.fromString(uuidString)
        assertEquals(uuidString, uuid.toString())
    }

    @Test
    fun testFromData() {
        val data = ByteArray(UUID.UUID_SIZE) { it.toByte() }
        val uuid = UUID.fromData(data)
        assertEquals(UUID.UUID_SIZE, uuid.data().size)
    }

    @Test
    fun testCborRoundtrip() {
        registerTags()
        val uuid = UUID.create()
        val cbor = uuid.taggedCbor()
        val decoded = UUID.fromTaggedCbor(cbor)
        assertEquals(uuid, decoded)
    }

    @Test
    fun testEquality() {
        val data = "0123456789abcdef0123456789abcdef".hexToByteArray()
        val uuid1 = UUID.fromData(data)
        val uuid2 = UUID.fromData(data)
        assertEquals(uuid1, uuid2)
    }
}
