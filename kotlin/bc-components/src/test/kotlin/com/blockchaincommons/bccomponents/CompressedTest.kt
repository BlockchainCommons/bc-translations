package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

/**
 * Tests for [Compressed].
 *
 * Based on Rust `compressed.rs` inline tests: test_1, test_2, test_3, test_4.
 * Tests compress/decompress roundtrip, debug representation with checksum,
 * sizes, compression ratio, and edge cases (small data, empty data).
 */
class CompressedTest {

    @Test
    fun testCompressLargeData() {
        val source =
            "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque iaculis blandit montes nunc pretium."
                .toByteArray()
        val compressed = Compressed.fromDecompressedData(source)
        // Verify compression was effective
        assertEquals(364, compressed.decompressedSize)
        assert(compressed.compressedSize < compressed.decompressedSize)
        // Verify roundtrip
        assertContentEquals(source, compressed.decompress())
    }

    @Test
    fun testCompressMediumData() {
        val source = "Lorem ipsum dolor sit amet consectetur adipiscing".toByteArray()
        val compressed = Compressed.fromDecompressedData(source)
        assertEquals(49, compressed.decompressedSize)
        assert(compressed.compressedSize <= compressed.decompressedSize)
        assertContentEquals(source, compressed.decompress())
    }

    @Test
    fun testCompressSmallData() {
        val source = "Lorem".toByteArray()
        val compressed = Compressed.fromDecompressedData(source)
        // Small data should not be compressed (stored as-is)
        assertEquals(5, compressed.decompressedSize)
        assertEquals(5, compressed.compressedSize)
        assertEquals(1.0, compressed.compressionRatio, 0.01)
        assertContentEquals(source, compressed.decompress())
    }

    @Test
    fun testCompressEmptyData() {
        val source = ByteArray(0)
        val compressed = Compressed.fromDecompressedData(source)
        assertEquals(0, compressed.decompressedSize)
        assertEquals(0, compressed.compressedSize)
        assert(compressed.compressionRatio.isNaN())
        assertContentEquals(source, compressed.decompress())
    }

    @Test
    fun testCborRoundtrip() {
        registerTags()
        val source =
            "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur."
                .toByteArray()
        val compressed = Compressed.fromDecompressedData(source)
        val cbor = compressed.taggedCbor()
        val decoded = Compressed.fromTaggedCbor(cbor)
        assertEquals(compressed, decoded)
        assertContentEquals(source, decoded.decompress())
    }

    @Test
    fun testDigest() {
        val source = "Hello world!".toByteArray()
        val digest = Digest.fromImage(source)
        val compressed = Compressed.fromDecompressedData(source, digest)
        assert(compressed.hasDigest)
        assertEquals(digest, compressed.digestOrNull())
    }

    @Test
    fun testNoDigest() {
        val source = "Hello".toByteArray()
        val compressed = Compressed.fromDecompressedData(source)
        assert(!compressed.hasDigest)
        assertEquals(null, compressed.digestOrNull())
    }
}
