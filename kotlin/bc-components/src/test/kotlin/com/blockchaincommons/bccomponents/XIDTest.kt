package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull
import kotlin.test.assertTrue

/**
 * Tests for [XID].
 *
 * Based on Rust `id/xid.rs` tests.
 */
class XIDTest {

    private val seedHex = "59f2293a5bce7d4de59e71b4207ac5d2"

    @Test
    fun testXidFromKey() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)
        val pubKeys = pkb.publicKeys()
        val xid = XID.fromSigningPublicKey(pubKeys.signingPublicKey)
        assertNotNull(xid)
        assertEquals(32, xid.data().size)
    }

    @Test
    fun testXidValidate() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)
        val signingPubKey = pkb.signingPublicKey()
        val xid = XID.fromSigningPublicKey(signingPubKey)

        // The XID should validate against the key it was created from
        assertTrue(xid.validate(signingPubKey))
    }

    @Test
    fun testXidCborRoundtrip() {
        registerTags()
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)
        val xid = XID.fromSigningPublicKey(pkb.signingPublicKey())

        val cbor = xid.taggedCbor()
        val decoded = XID.fromTaggedCbor(cbor)
        assertEquals(xid, decoded)
    }

    @Test
    fun testXidFromData() {
        val xidHex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037"
        val xid = XID.fromData(xidHex.hexToByteArray())
        assertEquals(32, xid.data().size)

        val cbor = xid.taggedCbor()
        val decoded = XID.fromTaggedCbor(cbor)
        assertEquals(xid, decoded)
    }

    @Test
    fun testXidFromHex() {
        registerTags()
        val xidHex = "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037"
        val xid = XID.fromHex(xidHex)
        assertEquals(xidHex, xid.hex)
    }

    @Test
    fun testXidEquality() {
        val seedData = seedHex.hexToByteArray()
        val pkb = PrivateKeyBase.fromData(seedData)
        val signingPubKey = pkb.signingPublicKey()

        val xid1 = XID.fromSigningPublicKey(signingPubKey)
        val xid2 = XID.fromSigningPublicKey(signingPubKey)
        assertEquals(xid1, xid2)
    }

    @Test
    fun testXidComparable() {
        val xid1 = XID.fromHex("0000000000000000000000000000000000000000000000000000000000000001")
        val xid2 = XID.fromHex("0000000000000000000000000000000000000000000000000000000000000002")
        assertTrue(xid1 < xid2)
    }
}
