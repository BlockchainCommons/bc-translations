package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/**
 * Tests for [CborJson].
 *
 * Based on Rust `json.rs` inline tests: test_json_creation, test_json_from_bytes,
 * test_json_empty, test_json_cbor_roundtrip, test_json_hex, test_json_debug,
 * test_json_clone, test_json_into_vec.
 */
class CborJsonTest {

    @Test
    fun testJsonCreation() {
        val json = CborJson.fromString("""{"key": "value"}""")
        assertEquals("""{"key": "value"}""", json.asString())
        assertEquals(16, json.size)
        assertFalse(json.isEmpty)
    }

    @Test
    fun testJsonFromBytes() {
        val data = "[1, 2, 3]".toByteArray()
        val json = CborJson.fromData(data)
        assertContentEquals(data, json.asBytes())
        assertEquals("[1, 2, 3]", json.asString())
    }

    @Test
    fun testJsonEmpty() {
        val json = CborJson.fromString("")
        assertTrue(json.isEmpty)
        assertEquals(0, json.size)
    }

    @Test
    fun testJsonCborRoundtrip() {
        registerTags()
        val json = CborJson.fromString("""{"name":"Alice","age":30}""")
        val cbor = json.taggedCbor()
        val json2 = CborJson.fromTaggedCbor(cbor)
        assertEquals(json, json2)
    }

    @Test
    fun testJsonHex() {
        val json = CborJson.fromString("test")
        val hex = json.hex
        val json2 = CborJson.fromHex(hex)
        assertEquals(json, json2)
    }

    @Test
    fun testJsonDebug() {
        val json = CborJson.fromString("""{"test":true}""")
        val str = json.toString()
        assertEquals("""JSON({"test":true})""", str)
    }

    @Test
    fun testJsonClone() {
        val json = CborJson.fromString("original")
        val json2 = CborJson.fromData(json.asBytes())
        assertEquals(json, json2)
    }

    @Test
    fun testJsonIntoByteArray() {
        val json = CborJson.fromString("data")
        val bytes = json.toByteArray()
        assertContentEquals("data".toByteArray(), bytes)
    }
}
