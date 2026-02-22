package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

/**
 * Tests for [SSKRShare], [sskrGenerate], and [sskrCombine].
 *
 * Based on the Rust `sskr_mod.rs` API surface and the sskr crate integration.
 * Tests cover share metadata parsing, generate/combine roundtrip, and CBOR
 * roundtrip.
 */
class SSKRShareTest {

    @Test
    fun testShareMetadata() {
        // Construct a share with known metadata bytes
        val data = byteArrayOf(
            0x12, 0x34,        // identifier: 0x1234
            0x21,              // group_threshold-1=2 (high nibble), group_count-1=1 (low nibble)
            0x31.toByte(),     // group_index=3 (high nibble), member_threshold-1=1 (low nibble)
            0x01,              // member_index=1 (low nibble)
            0xAA.toByte(), 0xBB.toByte(), 0xCC.toByte(), // share value
        )
        val share = SSKRShare.fromData(data)

        assertEquals(0x1234, share.identifier())
        assertEquals("1234", share.identifierHex())
        assertEquals(3, share.groupThreshold())
        assertEquals(2, share.groupCount())
        assertEquals(3, share.groupIndex())
        assertEquals(2, share.memberThreshold())
        assertEquals(1, share.memberIndex())
    }

    @Test
    fun testShareHexRoundtrip() {
        val hex = "1234213101aabbcc"
        val share = SSKRShare.fromHex(hex)
        assertEquals(hex, share.hex)
    }

    @Test
    fun testGenerateAndCombineSimple() {
        // Single group, 1 of 1
        val secretData = "0123456789abcdef".toByteArray() // 16 bytes
        val masterSecret = SSKRSecret(secretData)
        val group = SSKRGroupSpec(1, 1)
        val spec = SSKRSpec(1, listOf(group))

        val shares = sskrGenerate(spec, masterSecret)
        assertEquals(1, shares.size)
        assertEquals(1, shares[0].size)

        val recovered = sskrCombine(shares[0])
        assertContentEquals(secretData, recovered.toByteArray())
    }

    @Test
    fun testGenerateAndCombine2of3() {
        val secretData = "0123456789abcdef".toByteArray() // 16 bytes
        val masterSecret = SSKRSecret(secretData)
        val group = SSKRGroupSpec(2, 3)
        val spec = SSKRSpec(1, listOf(group))

        val shares = sskrGenerate(spec, masterSecret)
        assertEquals(1, shares.size)
        assertEquals(3, shares[0].size)

        // Use first 2 shares to recover
        val recoveryShares = listOf(shares[0][0], shares[0][1])
        val recovered = sskrCombine(recoveryShares)
        assertContentEquals(secretData, recovered.toByteArray())
    }

    @Test
    fun testGenerateAndCombineMultiGroup() {
        val secretData = "0123456789abcdef".toByteArray() // 16 bytes
        val masterSecret = SSKRSecret(secretData)
        val group1 = SSKRGroupSpec(2, 3)
        val group2 = SSKRGroupSpec(3, 5)
        val spec = SSKRSpec(2, listOf(group1, group2))

        val shares = sskrGenerate(spec, masterSecret)
        assertEquals(2, shares.size)
        assertEquals(3, shares[0].size)
        assertEquals(5, shares[1].size)

        // Collect shares that meet the threshold requirements:
        // 2 from group 1 + 3 from group 2
        val recoveryShares = listOf(
            shares[0][0], shares[0][1],
            shares[1][0], shares[1][1], shares[1][2],
        )
        val recovered = sskrCombine(recoveryShares)
        assertContentEquals(secretData, recovered.toByteArray())
    }

    @Test
    fun testShareCborRoundtrip() {
        registerTags()
        val secretData = "0123456789abcdef".toByteArray()
        val masterSecret = SSKRSecret(secretData)
        val group = SSKRGroupSpec(1, 1)
        val spec = SSKRSpec(1, listOf(group))

        val shares = sskrGenerate(spec, masterSecret)
        val share = shares[0][0]

        val cbor = share.taggedCbor()
        val decoded = SSKRShare.fromTaggedCbor(cbor)
        assertEquals(share, decoded)
    }
}
