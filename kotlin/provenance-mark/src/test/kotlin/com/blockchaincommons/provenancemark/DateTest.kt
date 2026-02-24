package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.CborDate
import java.time.Instant
import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals
import kotlin.test.assertFails

class DateTest {
    @Test
    fun test2ByteDates() {
        val baseDate = CborDate.fromYmdHms(2023, 6, 20, 0, 0, 0)
        val serialized = baseDate.serialize2Bytes()
        assertEquals("00d4", serialized.toHex())
        val deserialized = deserialize2Bytes(serialized)
        assertEquals(baseDate, deserialized)

        val minSerialized = byteArrayOf(0x00, 0x21)
        val minDate = CborDate.fromYmdHms(2023, 1, 1, 0, 0, 0)
        val deserializedMin = deserialize2Bytes(minSerialized)
        assertEquals(minDate, deserializedMin)

        val maxSerialized = byteArrayOf(0xff.toByte(), 0x9f.toByte())
        val deserializedMax = deserialize2Bytes(maxSerialized)
        val expectedMaxDate = CborDate.fromYmdHms(2150, 12, 31, 0, 0, 0)
        assertEquals(expectedMaxDate, deserializedMax)

        val invalidSerialized = byteArrayOf(0x00, 0x5e)
        assertFails { deserialize2Bytes(invalidSerialized) }
    }

    @Test
    fun test4ByteDates() {
        val baseDate = CborDate.fromYmdHms(2023, 6, 20, 12, 34, 56)
        val serialized = baseDate.serialize4Bytes()
        assertContentEquals(hex("2a41d470"), serialized)
        val deserialized = deserialize4Bytes(serialized)
        assertEquals(baseDate, deserialized)

        val minSerialized = hex("00000000")
        val minDate = CborDate.fromYmdHms(2001, 1, 1, 0, 0, 0)
        val deserializedMin = deserialize4Bytes(minSerialized)
        assertEquals(minDate, deserializedMin)

        val maxSerialized = hex("ffffffff")
        val deserializedMax = deserialize4Bytes(maxSerialized)
        val expectedMaxDate = CborDate.fromYmdHms(2137, 2, 7, 6, 28, 15)
        assertEquals(expectedMaxDate, deserializedMax)
    }

    @Test
    fun test6ByteDates() {
        val baseDate = CborDate.fromInstant(Instant.parse("2023-06-20T12:34:56.789Z"))
        val serialized = baseDate.serialize6Bytes()
        assertContentEquals(hex("00a51125d895"), serialized)
        val deserialized = deserialize6Bytes(serialized)
        assertEquals(baseDate, deserialized)

        val minSerialized = hex("000000000000")
        val minDate = CborDate.fromYmdHms(2001, 1, 1, 0, 0, 0)
        val deserializedMin = deserialize6Bytes(minSerialized)
        assertEquals(minDate, deserializedMin)

        val maxSerialized = hex("e5940a78a7ff")
        val deserializedMax = deserialize6Bytes(maxSerialized)
        val expectedMaxDate = CborDate.fromInstant(Instant.parse("9999-12-31T23:59:59.999Z"))
        assertEquals(expectedMaxDate, deserializedMax)

        val invalidSerialized = hex("e5940a78a800")
        assertFails { deserialize6Bytes(invalidSerialized) }
    }
}
