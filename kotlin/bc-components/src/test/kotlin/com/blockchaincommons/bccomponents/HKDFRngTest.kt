package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertContentEquals

/**
 * Tests for [HKDFRng].
 *
 * Based on Rust `hkdf_rng.rs` inline tests: test_hkdf_rng_next_bytes,
 * test_hkdf_rng_next_u32, test_hkdf_rng_next_u64, and
 * test_hkdf_rng_fill_bytes.
 *
 * Uses the same key material ("key_material") and salt ("salt") as the
 * Rust reference tests.
 */
class HKDFRngTest {

    @Test
    fun testHkdfRngNextBytes() {
        val rng = HKDFRng("key_material".toByteArray(), "salt")

        val page1 = rng.randomData(16)
        assertEquals("1032ac8ffea232a27c79fe381d7eb7e4", page1.toHexString())

        val page2 = rng.randomData(16)
        assertEquals("aeaaf727d35b6f338218391f9f8fa1f3", page2.toHexString())

        val page3 = rng.randomData(16)
        assertEquals("4348a59427711deb1e7d8a6959c6adb4", page3.toHexString())

        val page4 = rng.randomData(16)
        assertEquals("5d937a42cb5fb090fe1a1ec88f56e32b", page4.toHexString())
    }

    @Test
    fun testHkdfRngNextU32() {
        val rng = HKDFRng("key_material".toByteArray(), "salt")
        val v = rng.nextU32()
        assertEquals(2410426896u, v)
    }

    @Test
    fun testHkdfRngNextU64() {
        val rng = HKDFRng("key_material".toByteArray(), "salt")
        val v = rng.nextU64()
        assertEquals(11687583197195678224UL, v)
    }

    @Test
    fun testHkdfRngFillBytes() {
        val rng = HKDFRng("key_material".toByteArray(), "salt")
        val dest = ByteArray(16)
        rng.fillRandomData(dest)
        assertEquals("1032ac8ffea232a27c79fe381d7eb7e4", dest.toHexString())
    }
}
