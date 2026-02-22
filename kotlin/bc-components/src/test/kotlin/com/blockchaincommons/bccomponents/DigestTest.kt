package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNotEquals
import kotlin.test.assertTrue

/**
 * Tests for [Digest].
 *
 * Based on Rust `digest.rs` inline tests: test_digest, test_digest_from_hex,
 * test_ur, test_digest_equality, test_digest_inequality, and
 * test_invalid_hex_string.
 */
class DigestTest {

    @Test
    fun testDigest() {
        val data = "hello world"
        val digest = Digest.fromImage(data.toByteArray())
        assertEquals(Digest.DIGEST_SIZE, digest.data().size)
        assertEquals(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
            digest.hex,
        )
    }

    @Test
    fun testDigestFromHex() {
        val hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        val digest = Digest.fromHex(hex)
        assertEquals(Digest.DIGEST_SIZE, digest.data().size)
        assertEquals(hex, digest.hex)
        // Same as SHA-256 of "hello world"
        assertEquals(Digest.fromImage("hello world".toByteArray()), digest)
    }

    @Test
    fun testUr() {
        registerTags()
        val data = "hello world"
        val digest = Digest.fromImage(data.toByteArray())
        val urString = digest.urString()
        val expectedUrString =
            "ur:digest/hdcxrhgtdirhmugtfmayondmgmtstnkipyzssslrwsvlkngulawymhloylpsvowssnwlamnlatrs"
        assertEquals(expectedUrString, urString)
        val digest2 = Digest.fromUrString(urString)
        assertEquals(digest, digest2)
    }

    @Test
    fun testDigestEquality() {
        val hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        val digest1 = Digest.fromHex(hex)
        val digest2 = Digest.fromHex(hex)
        assertEquals(digest1, digest2)
    }

    @Test
    fun testDigestInequality() {
        val digest1 = Digest.fromHex(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
        )
        val digest2 = Digest.fromHex(
            "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
        )
        assertNotEquals(digest1, digest2)
    }

    @Test
    fun testInvalidHexString() {
        assertFailsWith<IllegalArgumentException> {
            Digest.fromHex("invalid_hex_string")
        }
    }

    @Test
    fun testValidate() {
        val data = "hello world"
        val digest = Digest.fromImage(data.toByteArray())
        assertTrue(digest.validate(data.toByteArray()))
    }

    @Test
    fun testShortDescription() {
        val digest = Digest.fromImage("hello world".toByteArray())
        assertEquals("b94d27b9", digest.shortDescription())
    }
}
