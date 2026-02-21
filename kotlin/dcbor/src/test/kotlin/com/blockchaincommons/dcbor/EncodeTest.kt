package com.blockchaincommons.dcbor

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue
import kotlin.test.assertFailsWith

class EncodeTest {

    private fun testCbor(cbor: Cbor, expectedDiag: String, expectedHex: String) {
        assertEquals(expectedHex, cbor.hex)
        assertEquals(expectedDiag, cbor.diagnosticFlat)
        // Round-trip
        val decoded = Cbor.tryFromHex(expectedHex)
        assertEquals(cbor, decoded)
    }

    // ---- Unsigned integers ----

    @Test
    fun testUnsigned0() = testCbor(0.toCbor(), "0", "00")

    @Test
    fun testUnsigned1() = testCbor(1.toCbor(), "1", "01")

    @Test
    fun testUnsigned23() = testCbor(23.toCbor(), "23", "17")

    @Test
    fun testUnsigned24() = testCbor(24.toCbor(), "24", "1818")

    @Test
    fun testUnsigned255() = testCbor(255.toCbor(), "255", "18ff")

    @Test
    fun testUnsigned256() = testCbor(256.toCbor(), "256", "190100")

    @Test
    fun testUnsigned65535() = testCbor(65535.toCbor(), "65535", "19ffff")

    @Test
    fun testUnsigned65536() = testCbor(65536.toCbor(), "65536", "1a00010000")

    @Test
    fun testUnsigned1000000() = testCbor(1000000.toCbor(), "1000000", "1a000f4240")

    @Test
    fun testUnsignedMaxU32() = testCbor(
        4294967295u.toCbor(), "4294967295", "1affffffff"
    )

    @Test
    fun testUnsignedMaxU32Plus1() = testCbor(
        4294967296L.toCbor(), "4294967296", "1b0000000100000000"
    )

    @Test
    fun testUnsignedMaxU64() = testCbor(
        ULong.MAX_VALUE.toCbor(), "18446744073709551615", "1bffffffffffffffff"
    )

    // ---- Negative integers ----

    @Test
    fun testNeg1() = testCbor((-1).toCbor(), "-1", "20")

    @Test
    fun testNeg10() = testCbor((-10).toCbor(), "-10", "29")

    @Test
    fun testNeg24() = testCbor((-24).toCbor(), "-24", "37")

    @Test
    fun testNeg25() = testCbor((-25).toCbor(), "-25", "3818")

    @Test
    fun testNeg100() = testCbor((-100).toCbor(), "-100", "3863")

    @Test
    fun testNeg1000() = testCbor((-1000).toCbor(), "-1000", "3903e7")

    // ---- Booleans ----

    @Test
    fun testFalse() = testCbor(false.toCbor(), "false", "f4")

    @Test
    fun testTrue() = testCbor(true.toCbor(), "true", "f5")

    // ---- Null ----

    @Test
    fun testNull() = testCbor(Cbor.`null`(), "null", "f6")

    // ---- Floats ----

    @Test
    fun testFloat0() = testCbor(0.0.toCbor(), "0", "00")

    @Test
    fun testFloatNeg0() = testCbor((-0.0).toCbor(), "0", "00")

    @Test
    fun testFloat1() = testCbor(1.0.toCbor(), "1", "01")

    @Test
    fun testFloat1_5() = testCbor(1.5.toCbor(), "1.5", "f93e00")

    @Test
    fun testFloatNeg4_1() = testCbor((-4.1).toCbor(), "-4.1", "fbc010666666666666")

    @Test
    fun testFloatPi() = testCbor(3.14159.toCbor(), "3.14159", "fb400921f9f01b866e")

    @Test
    fun testNaN() = testCbor(Double.NaN.toCbor(), "NaN", "f97e00")

    @Test
    fun testInfinity() = testCbor(Double.POSITIVE_INFINITY.toCbor(), "Infinity", "f97c00")

    @Test
    fun testNegInfinity() = testCbor(Double.NEGATIVE_INFINITY.toCbor(), "-Infinity", "f9fc00")

    // ---- Strings ----

    @Test
    fun testEmptyString() = testCbor("".toCbor(), "\"\"", "60")

    @Test
    fun testStringA() = testCbor("a".toCbor(), "\"a\"", "6161")

    @Test
    fun testStringHello() = testCbor("Hello".toCbor(), "\"Hello\"", "6548656c6c6f")

    // ---- Byte strings ----

    @Test
    fun testEmptyByteString() = testCbor(
        Cbor.fromByteString(byteArrayOf()),
        "h''",
        "40"
    )

    @Test
    fun testByteString() = testCbor(
        Cbor.fromByteString(byteArrayOf(1, 2, 3, 4)),
        "h'01020304'",
        "4401020304"
    )

    // ---- Arrays ----

    @Test
    fun testEmptyArray() = testCbor(
        listOf<Cbor>().toCbor(),
        "[]",
        "80"
    )

    @Test
    fun testArray123() = testCbor(
        listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor(),
        "[1, 2, 3]",
        "83010203"
    )

    @Test
    fun testArrayMixed() = testCbor(
        listOf(1.toCbor(), "Hello".toCbor(), listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor()).toCbor(),
        "[1, \"Hello\", [1, 2, 3]]",
        "83016548656c6c6f83010203"
    )

    // ---- Maps ----

    @Test
    fun testEmptyMap() {
        val map = CborMap()
        testCbor(Cbor.fromMap(map), "{}", "a0")
    }

    @Test
    fun testMapIntKeys() {
        val map = CborMap()
        map.insert(1.toCbor(), 2.toCbor())
        map.insert(3.toCbor(), 4.toCbor())
        testCbor(Cbor.fromMap(map), "{1: 2, 3: 4}", "a201020304")
    }

    @Test
    fun testMapStringKey() {
        val map = CborMap()
        map.insert("key".toCbor(), 123.toCbor())
        testCbor(Cbor.fromMap(map), "{\"key\": 123}", "a1636b6579187b")
    }

    // ---- Tagged values ----

    @Test
    fun testTagged() {
        val tagged = Cbor.taggedValue(1uL, "Hello".toCbor())
        testCbor(tagged, "1(\"Hello\")", "c16548656c6c6f")
    }

    // ---- Error cases ----

    @Test
    fun testNonCanonicalInteger() {
        // 0 encoded with one-byte additional info (0x1800) instead of inline (0x00)
        assertFailsWith<CborException.NonCanonicalNumeric> {
            Cbor.tryFromHex("1800")
        }
    }

    @Test
    fun testUnusedData() {
        assertFailsWith<CborException.UnusedData> {
            Cbor.tryFromHex("0100")
        }
    }

    @Test
    fun testDuplicateMapKey() {
        // Map with duplicate key 1: {1: 2, 1: 3}
        assertFailsWith<CborException.DuplicateMapKey> {
            Cbor.tryFromHex("a201020103")
        }
    }

    @Test
    fun testMisorderedMapKey() {
        // Map with keys out of order: {2: 2, 1: 1}
        assertFailsWith<CborException.MisorderedMapKey> {
            Cbor.tryFromHex("a202020101")
        }
    }

    // ---- Numeric reduction ----

    @Test
    fun testFloatIntegerReduction() {
        // 42.0 should encode as integer 42
        assertEquals("182a", 42.0.toCbor().hex)
        assertEquals("182a", 42.0f.toCbor().hex)
    }
}
